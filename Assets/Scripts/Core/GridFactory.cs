using System;

namespace BlastGame.Core
{
    /// <summary>Builds grids.</summary>
    public static class GridFactory
    {
        public const int MaxColorCount = 6;

        public static Grid CreateRandom(int width, int height, int colorCount, IRandomSource random)
        {
            // Validate everything before touching the random source.
            if (width < Grid.MinSize || width > Grid.MaxSize)
                throw new ArgumentOutOfRangeException(nameof(width));
            if (height < Grid.MinSize || height > Grid.MaxSize)
                throw new ArgumentOutOfRangeException(nameof(height));
            if (colorCount < 1 || colorCount > MaxColorCount)
                throw new ArgumentOutOfRangeException(nameof(colorCount));
            if (random == null)
                throw new ArgumentNullException(nameof(random));

            var grid = new Grid(width, height);
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    grid.Set(x, y, (TileColor)random.Next(colorCount));
                }
            }
            return grid;
        }
    }
}
