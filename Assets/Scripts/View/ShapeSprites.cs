using UnityEngine;

namespace BlastGame.View
{
    /// <summary>
    /// Primitive shapes drawn into small textures in code, once per run (never per tap).
    /// Every sprite is one world unit wide and centered; white shapes are tinted by the renderer.
    /// </summary>
    public static class ShapeSprites
    {
        private const int Size = 64;

        private static bool _built;
        private static Sprite _square;
        private static Sprite _disc;
        private static Sprite _ring;
        private static Sprite _dashedRing;
        private static Sprite _sphere;
        private static Sprite _diamondH;
        private static Sprite _diamondV;
        private static Sprite _arrowsH;
        private static Sprite _arrowsV;
        private static Sprite _arrowHead;
        private static Sprite _trail;

        /// <summary>Plain 1x1 square (tiles, beams, the board mask).</summary>
        public static Sprite Square { get { Build(); return _square; } }
        /// <summary>Soft-edged filled circle.</summary>
        public static Sprite Disc { get { Build(); return _disc; } }
        /// <summary>Thin circle outline.</summary>
        public static Sprite Ring { get { Build(); return _ring; } }
        /// <summary>Circle outline in three arcs, so a slow spin can be seen.</summary>
        public static Sprite DashedRing { get { Build(); return _dashedRing; } }
        /// <summary>Shaded ball, white so it takes the Orb color.</summary>
        public static Sprite Sphere { get { Build(); return _sphere; } }
        /// <summary>Long rhombus, lying (horizontal) or standing (vertical).</summary>
        public static Sprite Diamond(bool vertical) { Build(); return vertical ? _diamondV : _diamondH; }
        /// <summary>Two arrow heads pointing away from the center along the rocket's axis.</summary>
        public static Sprite Arrows(bool vertical) { Build(); return vertical ? _arrowsV : _arrowsH; }
        /// <summary>One arrow head pointing right (+x); rotate for other directions.</summary>
        public static Sprite ArrowHead { get { Build(); return _arrowHead; } }
        /// <summary>Horizontal band fading from clear (left) to solid (right).</summary>
        public static Sprite Trail { get { Build(); return _trail; } }

        private static void Build()
        {
            if (_built) return;
            _built = true;

            var t = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            t.SetPixel(0, 0, Color.white);
            t.Apply();
            t.filterMode = FilterMode.Point;
            _square = Sprite.Create(t, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);

            _disc = Make((x, y) => Edge(0.47f - Length(x, y)));
            _ring = Make((x, y) => Edge(0.035f - Mathf.Abs(Length(x, y) - 0.43f)));
            _dashedRing = Make((x, y) =>
            {
                float a = Mathf.Repeat(Mathf.Atan2(y, x) * Mathf.Rad2Deg, 120f);
                float arc = Edge(Mathf.Min(a - 8f, 112f - a) * 0.01f).a;
                return new Color(1f, 1f, 1f, arc * Edge(0.03f - Mathf.Abs(Length(x, y) - 0.45f)).a);
            });
            _sphere = Make((x, y) =>
            {
                float alpha = Edge(0.36f - Length(x, y)).a;
                // Light from the top left: brighter there, darker at the bottom right rim.
                float light = Mathf.Clamp01(1f - Length(x + 0.12f, y - 0.12f) * 1.4f);
                float shade = 0.62f + 0.38f * light;
                return new Color(shade, shade, shade, alpha);
            });
            _diamondH = Make((x, y) => Edge((1f - Mathf.Abs(x) / 0.30f - Mathf.Abs(y) / 0.20f) * 0.2f));
            _diamondV = Make((x, y) => Edge((1f - Mathf.Abs(y) / 0.30f - Mathf.Abs(x) / 0.20f) * 0.2f));
            _arrowsH = Make((x, y) => ArrowPair(x, y));
            _arrowsV = Make((x, y) => ArrowPair(y, x));
            _arrowHead = Make((x, y) => Edge(Mathf.Min(0.42f - x - Mathf.Abs(y) * 1.3f, x + 0.18f)));
            _trail = Make((x, y) => new Color(1f, 1f, 1f, (x + 0.5f) * Edge(0.18f - Mathf.Abs(y)).a));
        }

        // Two small triangles past the rhombus tips, tips outward.
        private static Color ArrowPair(float along, float across)
        {
            float a = Mathf.Abs(along);
            return Edge(Mathf.Min(0.48f - a - Mathf.Abs(across), a - 0.34f));
        }

        private delegate Color Shader(float x, float y);

        // Samples the shader at pixel centers in [-0.5, 0.5] x [-0.5, 0.5].
        private static Sprite Make(Shader shader)
        {
            var texture = new Texture2D(Size, Size, TextureFormat.RGBA32, false);
            var pixels = new Color[Size * Size];
            for (int py = 0; py < Size; py++)
                for (int px = 0; px < Size; px++)
                {
                    float x = (px + 0.5f) / Size - 0.5f;
                    float y = (py + 0.5f) / Size - 0.5f;
                    pixels[py * Size + px] = shader(x, y);
                }
            texture.SetPixels(pixels);
            texture.wrapMode = TextureWrapMode.Clamp;
            texture.filterMode = FilterMode.Bilinear;
            texture.Apply();
            return Sprite.Create(texture, new Rect(0, 0, Size, Size), new Vector2(0.5f, 0.5f), Size);
        }

        // White with an anti-aliased edge: positive distance is inside.
        private static Color Edge(float inside)
        {
            return new Color(1f, 1f, 1f, Mathf.Clamp01(inside * Size + 0.5f));
        }

        private static float Length(float x, float y)
        {
            return Mathf.Sqrt(x * x + y * y);
        }
    }
}
