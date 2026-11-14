using System;
using System.Collections.Generic;
using System.IO;
using BlastGame.View;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Turns the approved obstacle drafts (white-background JPGs in Builds/art-draft, kept out of git) into
/// transparent PNG sprites in Assets/Art/Obstacles and links them in Assets/Resources/ObstacleArt.asset.
/// Repeatable: run it again after a draft changes.
/// Menu: Tools > BlastGame > Import Obstacle Art.
/// Batchmode (Editor closed): Unity.exe -batchmode -quit -projectPath . -executeMethod ObstacleArtImporter.Run -logFile Logs/obstacle-art.log
/// </summary>
public static class ObstacleArtImporter
{
    private const string SourceFolder = "Builds/art-draft";
    private const string OutputFolder = "Assets/Art/Obstacles";
    private const string ArtAssetPath = "Assets/Resources/ObstacleArt.asset";
    private const int OutputSize = 256;   // pixels; the sprite is one world unit wide
    private const float Padding = 0.04f;  // share of the output left clear on each side

    // Background: how far from white (0..255, by the darkest channel) a pixel may be and still count as
    // background (fully clear below Clear, feathered up to Edge).
    private const int Clear = 22;
    private const int Edge = 70;

    [MenuItem("Tools/BlastGame/Import Obstacle Art")]
    public static void RunFromMenu()
    {
        try
        {
            ImportAll();
            EditorUtility.DisplayDialog("Obstacle art", "Obstacle sprites updated.", "OK");
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
            EditorUtility.DisplayDialog("Obstacle art", ex.Message, "OK");
        }
    }

    /// <summary>Batchmode entry point: exits 0 on success, 1 on any error.</summary>
    public static void Run()
    {
        int code = 0;
        try
        {
            ImportAll();
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
            code = 1;
        }
        if (Application.isBatchMode)
            EditorApplication.Exit(code);
    }

    private static void ImportAll()
    {
        Directory.CreateDirectory(OutputFolder);
        Directory.CreateDirectory(Path.GetDirectoryName(ArtAssetPath));

        // Pinata and Anchor: opaque art. The Anchor's ring hole is a white pocket the border cannot reach,
        // so it gets its own seed (x, y from the image's top-left, as shares of the size).
        Write(Cutout(Load("pinata-v1.jpg"), null), "pinata.png");
        Write(Cutout(Load("anchor-v1.jpg"), new[] { new Vector2(0.5f, 0.16f) }), "anchor.png");

        // Water balloon: the outline and knot stay, the body becomes see-through water with its shine kept.
        Write(Water(Cutout(Load("waterballoon-v1.jpg"), null)), "waterballoon.png");

        // Crate: one picture with the 1, 2 and 3 layer looks from left to right.
        Img crates = Cutout(Load("crate-v2.jpg"), null);
        int third = crates.W / 3;
        for (int i = 0; i < 3; i++)
            Write(crates.Slice(i * third, i == 2 ? crates.W - 2 * third : third), "crate" + (i + 1) + ".png");

        AssetDatabase.Refresh();
        foreach (string name in new[] { "pinata", "anchor", "waterballoon", "crate1", "crate2", "crate3" })
            ConfigureSprite(OutputFolder + "/" + name + ".png");

        var art = AssetDatabase.LoadAssetAtPath<ObstacleArt>(ArtAssetPath);
        if (art == null)
        {
            art = ScriptableObject.CreateInstance<ObstacleArt>();
            AssetDatabase.CreateAsset(art, ArtAssetPath);
        }
        art.Pinata = LoadSprite("pinata");
        art.Anchor = LoadSprite("anchor");
        art.WaterBalloon = LoadSprite("waterballoon");
        art.Crate1 = LoadSprite("crate1");
        art.Crate2 = LoadSprite("crate2");
        art.Crate3 = LoadSprite("crate3");
        EditorUtility.SetDirty(art);
        AssetDatabase.SaveAssets();
        Debug.Log("Obstacle art: 6 sprites written to " + OutputFolder + " and linked in " + ArtAssetPath + ".");
    }

    // ---------- Image in floats, top-left origin, straight (not premultiplied) alpha. ----------

    private sealed class Img
    {
        public readonly int W, H;
        public readonly Color[] P; // row 0 = top

        public Img(int w, int h)
        {
            W = w;
            H = h;
            P = new Color[w * h];
        }

        public Color this[int x, int y]
        {
            get { return P[y * W + x]; }
            set { P[y * W + x] = value; }
        }

        public Img Slice(int x0, int width)
        {
            var img = new Img(width, H);
            for (int y = 0; y < H; y++)
                for (int x = 0; x < width; x++)
                    img[x, y] = this[x0 + x, y];
            return img;
        }
    }

    private static Img Load(string file)
    {
        string path = Path.Combine(SourceFolder, file);
        if (!File.Exists(path))
            throw new FileNotFoundException("Draft not found: " + path);
        var tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
        if (!tex.LoadImage(File.ReadAllBytes(path)))
            throw new InvalidOperationException("Could not read " + path);
        Color[] src = tex.GetPixels(); // row 0 = bottom
        var img = new Img(tex.width, tex.height);
        for (int y = 0; y < img.H; y++)
            Array.Copy(src, (img.H - 1 - y) * img.W, img.P, y * img.W, img.W);
        UnityEngine.Object.DestroyImmediate(tex);
        return img;
    }

    /// <summary>How far a pixel is from white, 0..255, by its darkest channel.</summary>
    private static float Distance(Color c)
    {
        return (1f - Mathf.Min(c.r, Mathf.Min(c.g, c.b))) * 255f;
    }

    /// <summary>
    /// Clears the white background: a flood from every border pixel (and the extra seeds) through
    /// near-white pixels. Pixels near the limit get partial alpha, their color un-mixed from white,
    /// so the outline keeps a soft edge.
    /// </summary>
    private static Img Cutout(Img img, Vector2[] seeds)
    {
        var seen = new bool[img.W * img.H];
        var stack = new Stack<int>();
        for (int x = 0; x < img.W; x++)
        {
            Push(img, stack, seen, x, 0);
            Push(img, stack, seen, x, img.H - 1);
        }
        for (int y = 0; y < img.H; y++)
        {
            Push(img, stack, seen, 0, y);
            Push(img, stack, seen, img.W - 1, y);
        }
        if (seeds != null)
            foreach (Vector2 s in seeds)
                Push(img, stack, seen, Mathf.RoundToInt(s.x * (img.W - 1)), Mathf.RoundToInt(s.y * (img.H - 1)));

        while (stack.Count > 0)
        {
            int i = stack.Pop();
            int x = i % img.W, y = i / img.W;
            Color c = img.P[i];
            float d = Distance(c);
            float a = Mathf.Clamp01((d - Clear) / (Edge - Clear));
            img.P[i] = a <= 0f ? new Color(1f, 1f, 1f, 0f) : Unmix(c, a);
            if (d >= Edge * 0.85f) continue; // the soft rim: do not spread past it
            Push(img, stack, seen, x - 1, y);
            Push(img, stack, seen, x + 1, y);
            Push(img, stack, seen, x, y - 1);
            Push(img, stack, seen, x, y + 1);
        }
        return img;
    }

    private static void Push(Img img, Stack<int> stack, bool[] seen, int x, int y)
    {
        if (x < 0 || y < 0 || x >= img.W || y >= img.H) return;
        int i = y * img.W + x;
        if (seen[i]) return;
        if (Distance(img.P[i]) >= Edge) return;
        seen[i] = true;
        stack.Push(i);
    }

    /// <summary>The color that, drawn with alpha a over white, gives c.</summary>
    private static Color Unmix(Color c, float a)
    {
        return new Color(
            Mathf.Clamp01((c.r - (1f - a)) / a),
            Mathf.Clamp01((c.g - (1f - a)) / a),
            Mathf.Clamp01((c.b - (1f - a)) / a),
            a * c.a);
    }

    /// <summary>
    /// Water balloon body: light pixels become see-through (a thin white film in the middle, more tint
    /// toward the rim); dark ones (outline, knot) stay solid; shine spots (whiter than their surroundings)
    /// stay bright.
    /// </summary>
    private static Img Water(Img img)
    {
        const float Solid = 0.45f;  // un-mix alpha from which a pixel stays solid
        const float Film = 0.16f;   // alpha of the white middle
        const int Blur = 24;        // pixels; the "surroundings" for shine

        // Whiteness and its local average (box blur through a summed-area table).
        var white = new float[img.W * img.H];
        for (int i = 0; i < white.Length; i++)
            white[i] = Mathf.Min(img.P[i].r, Mathf.Min(img.P[i].g, img.P[i].b));
        var sum = new double[(img.W + 1) * (img.H + 1)];
        for (int y = 0; y < img.H; y++)
            for (int x = 0; x < img.W; x++)
                sum[(y + 1) * (img.W + 1) + x + 1] = white[y * img.W + x] + sum[y * (img.W + 1) + x + 1]
                                                     + sum[(y + 1) * (img.W + 1) + x] - sum[y * (img.W + 1) + x];

        for (int y = 0; y < img.H; y++)
            for (int x = 0; x < img.W; x++)
            {
                int i = y * img.W + x;
                Color c = img.P[i];
                if (c.a < 1f) continue; // background or its soft rim

                float a0 = 1f - white[i]; // un-mix alpha over white
                if (a0 >= Solid) continue;

                int x0 = Mathf.Max(0, x - Blur), x1 = Mathf.Min(img.W, x + Blur + 1);
                int y0 = Mathf.Max(0, y - Blur), y1 = Mathf.Min(img.H, y + Blur + 1);
                double area = (x1 - x0) * (y1 - y0);
                double avg = (sum[y1 * (img.W + 1) + x1] - sum[y0 * (img.W + 1) + x1]
                              - sum[y1 * (img.W + 1) + x0] + sum[y0 * (img.W + 1) + x0]) / area;
                float shine = Mathf.Clamp01(((float)(white[i] - avg) - 0.02f) * 9f);

                float a = Mathf.Lerp(Film, 1f, Mathf.Clamp01(a0 / Solid) * 0.85f);
                // Near-white pixels carry mostly JPG noise once un-mixed, so they fade to plain white.
                Color tint = Unmix(c, Mathf.Max(a0, 0.12f));
                Color water = Color.Lerp(Color.white, tint, Mathf.SmoothStep(0f, 1f, (a0 - 0.04f) / 0.14f));
                water.a = a;
                Color result = Color.Lerp(water, Color.white, shine);
                result.a = Mathf.Max(a, shine * 0.9f);
                img.P[i] = result;
            }
        return img;
    }

    /// <summary>Crops to the visible pixels, centers them in a square with a small margin and resizes to OutputSize.</summary>
    private static void Write(Img img, string file)
    {
        int minX = img.W, minY = img.H, maxX = -1, maxY = -1;
        for (int y = 0; y < img.H; y++)
            for (int x = 0; x < img.W; x++)
                if (img[x, y].a > 0.02f)
                {
                    if (x < minX) minX = x;
                    if (x > maxX) maxX = x;
                    if (y < minY) minY = y;
                    if (y > maxY) maxY = y;
                }
        if (maxX < 0)
            throw new InvalidOperationException(file + ": nothing left after removing the background.");

        float side = Mathf.Max(maxX - minX + 1, maxY - minY + 1) / (1f - 2f * Padding);
        float cx = (minX + maxX + 1) * 0.5f, cy = (minY + maxY + 1) * 0.5f;
        float scale = side / OutputSize; // source pixels per output pixel

        var tex = new Texture2D(OutputSize, OutputSize, TextureFormat.RGBA32, false);
        var outPixels = new Color[OutputSize * OutputSize];
        for (int oy = 0; oy < OutputSize; oy++)
            for (int ox = 0; ox < OutputSize; ox++)
            {
                // Box filter over the source area, with premultiplied alpha so clear pixels add no color.
                float sx0 = cx - side * 0.5f + ox * scale, sy0 = cy - side * 0.5f + oy * scale;
                int ix0 = Mathf.FloorToInt(sx0), iy0 = Mathf.FloorToInt(sy0);
                int ix1 = Mathf.Max(ix0 + 1, Mathf.FloorToInt(sx0 + scale)), iy1 = Mathf.Max(iy0 + 1, Mathf.FloorToInt(sy0 + scale));
                float r = 0f, g = 0f, b = 0f, a = 0f;
                int n = 0;
                for (int sy = iy0; sy < iy1; sy++)
                    for (int sx = ix0; sx < ix1; sx++)
                    {
                        n++;
                        if (sx < 0 || sy < 0 || sx >= img.W || sy >= img.H) continue;
                        Color c = img[sx, sy];
                        r += c.r * c.a;
                        g += c.g * c.a;
                        b += c.b * c.a;
                        a += c.a;
                    }
                Color o = a > 0f ? new Color(r / a, g / a, b / a, a / n) : new Color(1f, 1f, 1f, 0f);
                outPixels[(OutputSize - 1 - oy) * OutputSize + ox] = o; // back to bottom-up rows
            }
        tex.SetPixels(outPixels);
        tex.Apply();
        File.WriteAllBytes(Path.Combine(OutputFolder, file), tex.EncodeToPNG());
        UnityEngine.Object.DestroyImmediate(tex);
    }

    private static void ConfigureSprite(string path)
    {
        var importer = (TextureImporter)AssetImporter.GetAtPath(path);
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.spritePixelsPerUnit = OutputSize;
        importer.alphaIsTransparency = true;
        importer.mipmapEnabled = false;
        importer.wrapMode = TextureWrapMode.Clamp;
        importer.filterMode = FilterMode.Bilinear;
        importer.maxTextureSize = OutputSize;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.SaveAndReimport();
    }

    private static Sprite LoadSprite(string name)
    {
        string path = OutputFolder + "/" + name + ".png";
        var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
        if (sprite == null)
            throw new InvalidOperationException("No sprite at " + path);
        return sprite;
    }
}
