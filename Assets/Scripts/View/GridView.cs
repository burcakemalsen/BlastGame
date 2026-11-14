using System.Collections;
using BlastGame.Core;
using UnityEngine;
using Grid = BlastGame.Core.Grid; // UnityEngine.Grid also exists

namespace BlastGame.View
{
    /// <summary>
    /// Draws a grid as colored squares. Cell (0, 0) is at the world origin; spacing is 1 unit.
    /// Animates a turn from its TurnResult: blasted tiles shrink away, then moved and new tiles fall.
    /// </summary>
    public sealed class GridView : MonoBehaviour
    {
        private const float TileSize = 0.9f;
        private const float BlastDuration = 0.12f; // seconds for blasted tiles to shrink away
        private const float FallDuration = 0.35f;  // seconds for every falling tile, whatever the distance

        private static Sprite _squareSprite; // created once, shared by every tile and every Build

        private Grid _grid;
        private SpriteRenderer[,] _tiles;
        private Vector3[,] _fallStart; // reused by every turn
        private int[] _newInColumn;    // reused by every turn
        private bool _isAnimating;

        /// <summary>True while a turn is being animated; taps should be ignored meanwhile.</summary>
        public bool IsAnimating
        {
            get { return _isAnimating; }
        }

        public void Build(Grid grid)
        {
            _grid = grid;
            _tiles = new SpriteRenderer[grid.Width, grid.Height];
            _fallStart = new Vector3[grid.Width, grid.Height];
            _newInColumn = new int[grid.Width];
            if (_squareSprite == null) _squareSprite = CreateSquareSprite();
            Sprite sprite = _squareSprite;
            for (int y = 0; y < grid.Height; y++)
            {
                for (int x = 0; x < grid.Width; x++)
                {
                    var tile = new GameObject("Tile " + x + "," + y);
                    tile.transform.SetParent(transform, false);
                    tile.transform.localPosition = CellPosition(x, y);
                    tile.transform.localScale = new Vector3(TileSize, TileSize, 1f);

                    var sr = tile.AddComponent<SpriteRenderer>();
                    sr.sprite = sprite;
                    sr.maskInteraction = SpriteMaskInteraction.VisibleInsideMask;
                    _tiles[x, y] = sr;
                }
            }
            BuildMask(sprite, grid.Width, grid.Height);
            Refresh();
        }

        /// <summary>Syncs every tile with the grid: empty cells are hidden, colored ones show their color.</summary>
        public void Refresh()
        {
            for (int y = 0; y < _grid.Height; y++)
            {
                for (int x = 0; x < _grid.Width; x++)
                {
                    Cell cell = _grid.Get(x, y);
                    SpriteRenderer sr = _tiles[x, y];
                    sr.enabled = !cell.IsEmpty;
                    if (!cell.IsEmpty) sr.color = TileColors.ToColor(cell.Color);
                }
            }
        }

        /// <summary>
        /// Shows how the grid reached its current state. The grid is already final; the tiles still show
        /// the state before the tap, so the blasted ones shrink first, then every tile takes its final
        /// color and falls into its cell.
        /// </summary>
        public void AnimateTurn(TurnResult result)
        {
            if (_isAnimating || !result.HasBlasted) return;
            StartCoroutine(AnimateTurnRoutine(result));
        }

        private IEnumerator AnimateTurnRoutine(TurnResult result)
        {
            _isAnimating = true;

            // 1. Blasted tiles shrink to nothing.
            for (float t = 0f; t < BlastDuration; t += Time.deltaTime)
            {
                float s = TileSize * (1f - t / BlastDuration);
                foreach (CellPos p in result.Blasted)
                    _tiles[p.X, p.Y].transform.localScale = new Vector3(s, s, 1f);
                yield return null;
            }

            // 2. Every tile shows its final cell; moved and new tiles start where they come from.
            for (int y = 0; y < _grid.Height; y++)
                for (int x = 0; x < _grid.Width; x++)
                {
                    _fallStart[x, y] = CellPosition(x, y);
                    _tiles[x, y].transform.localScale = new Vector3(TileSize, TileSize, 1f);
                }
            foreach (CellMove m in result.Moves)
                _fallStart[m.To.X, m.To.Y] = CellPosition(m.From.X, m.From.Y);
            // New tiles in a column fill its top cells; they start just above the grid, in the same order,
            // so each one is lifted by the number of new tiles in its column.
            for (int x = 0; x < _grid.Width; x++) _newInColumn[x] = 0;
            foreach (CellPos p in result.Filled) _newInColumn[p.X]++;
            foreach (CellPos p in result.Filled)
                _fallStart[p.X, p.Y] = CellPosition(p.X, p.Y + _newInColumn[p.X]);
            Refresh();
            ApplyFall(0f);

            // 3. Everything falls together, slow start and fast end (cubic ease-in).
            for (float t = 0f; t < FallDuration; t += Time.deltaTime)
            {
                float k = t / FallDuration;
                ApplyFall(k * k * k);
                yield return null;
            }
            ApplyFall(1f);

            _isAnimating = false;
        }

        private void ApplyFall(float amount)
        {
            for (int y = 0; y < _grid.Height; y++)
                for (int x = 0; x < _grid.Width; x++)
                    _tiles[x, y].transform.localPosition =
                        Vector3.LerpUnclamped(_fallStart[x, y], CellPosition(x, y), amount);
        }

        private static Vector3 CellPosition(int x, int y)
        {
            return new Vector3(x, y, 0f);
        }

        // Tiles are visible only inside this rectangle, so new tiles entering from above stay hidden.
        private void BuildMask(Sprite sprite, int width, int height)
        {
            var maskGo = new GameObject("GridMask");
            maskGo.transform.SetParent(transform, false);
            maskGo.transform.localPosition = new Vector3((width - 1) * 0.5f, (height - 1) * 0.5f, 0f);
            maskGo.transform.localScale = new Vector3(width, height, 1f);
            maskGo.AddComponent<SpriteMask>().sprite = sprite;
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
