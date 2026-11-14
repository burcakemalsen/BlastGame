using System;

namespace BlastGame.Core
{
    /// <summary>Game board model. (0, 0) is the bottom-left cell; y grows upward.</summary>
    public sealed class Grid
    {
        public const int MinSize = 1;
        public const int MaxSize = 16;

        private readonly Cell[,] _cells;

        public Grid(int width, int height)
        {
            if (width < MinSize || width > MaxSize)
                throw new ArgumentOutOfRangeException(nameof(width));
            if (height < MinSize || height > MaxSize)
                throw new ArgumentOutOfRangeException(nameof(height));

            Width = width;
            Height = height;
            _cells = new Cell[width, height];
        }

        public int Width { get; }

        public int Height { get; }

        public bool IsInside(int x, int y)
        {
            return x >= 0 && x < Width && y >= 0 && y < Height;
        }

        /// <summary>Returns what the cell holds.</summary>
        public Cell Get(int x, int y)
        {
            ThrowIfOutside(x, y);
            return _cells[x, y];
        }

        public void Set(int x, int y, Cell cell)
        {
            ThrowIfOutside(x, y);
            _cells[x, y] = cell;
        }

        private void ThrowIfOutside(int x, int y)
        {
            if (x < 0 || x >= Width)
                throw new ArgumentOutOfRangeException(nameof(x), "Cell (" + x + ", " + y + ") is outside the grid.");
            if (y < 0 || y >= Height)
                throw new ArgumentOutOfRangeException(nameof(y), "Cell (" + x + ", " + y + ") is outside the grid.");
        }
    }
}
