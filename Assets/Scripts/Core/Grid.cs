using System;

namespace BlastGame.Core
{
    /// <summary>Game board model. (0, 0) is the bottom-left cell; y grows upward.</summary>
    public sealed class Grid
    {
        public const int MinSize = 1;
        public const int MaxSize = 16;

        private readonly TileColor?[,] _cells;

        public Grid(int width, int height)
        {
            if (width < MinSize || width > MaxSize)
                throw new ArgumentOutOfRangeException(nameof(width));
            if (height < MinSize || height > MaxSize)
                throw new ArgumentOutOfRangeException(nameof(height));

            Width = width;
            Height = height;
            _cells = new TileColor?[width, height];
        }

        public int Width { get; }

        public int Height { get; }

        public bool IsInside(int x, int y)
        {
            return x >= 0 && x < Width && y >= 0 && y < Height;
        }

        /// <summary>Returns the cell color; null means the cell is empty.</summary>
        public TileColor? Get(int x, int y)
        {
            ThrowIfOutside(x, y);
            return _cells[x, y];
        }

        public void Set(int x, int y, TileColor? color)
        {
            ThrowIfOutside(x, y);
            if (color.HasValue && !Enum.IsDefined(typeof(TileColor), color.Value))
                throw new ArgumentOutOfRangeException(nameof(color), "Undefined tile color: " + (int)color.Value + ".");
            _cells[x, y] = color;
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
