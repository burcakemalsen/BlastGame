using BlastGame.Core;
using UnityEngine;
using Grid = BlastGame.Core.Grid; // UnityEngine.Grid also exists

namespace BlastGame.View
{
    /// <summary>Draws a grid as colored squares. Cell (0, 0) is at the world origin; spacing is 1 unit.</summary>
    public sealed class GridView : MonoBehaviour
    {
        private const float TileSize = 0.9f;

        public void Build(Grid grid)
        {
            Sprite sprite = CreateSquareSprite();
            for (int y = 0; y < grid.Height; y++)
            {
                for (int x = 0; x < grid.Width; x++)
                {
                    TileColor? color = grid.Get(x, y);
                    if (color == null) continue;

                    var tile = new GameObject("Tile " + x + "," + y);
                    tile.transform.SetParent(transform, false);
                    tile.transform.localPosition = new Vector3(x, y, 0f);
                    tile.transform.localScale = new Vector3(TileSize, TileSize, 1f);

                    var sr = tile.AddComponent<SpriteRenderer>();
                    sr.sprite = sprite;
                    sr.color = TileColors.ToColor(color.Value);
                }
            }
        }

        // 1x1 white texture; with 1 pixel per unit the sprite is exactly one world unit wide.
        private static Sprite CreateSquareSprite()
        {
            var texture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            texture.SetPixel(0, 0, Color.white);
            texture.Apply();
            texture.filterMode = FilterMode.Point;
            return Sprite.Create(texture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
        }
    }
}
