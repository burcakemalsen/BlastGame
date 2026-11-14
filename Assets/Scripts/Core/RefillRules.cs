using System;
using System.Collections.Generic;

namespace BlastGame.Core
{
    /// <summary>Rules for filling empty cells with random colors.</summary>
    public static class RefillRules
    {
        /// <summary>
        /// Fills every empty cell (y ascending, then x ascending) with a color from the random source
        /// and returns the filled cells in that order.
        /// </summary>
        public static IReadOnlyList<CellPos> Fill(Grid grid, int colorCount, IRandomSource random)
        {
            // Validate everything before touching the random source.
            if (grid == null)
                throw new ArgumentNullException(nameof(grid));
            if (colorCount < 1 || colorCount > GridFactory.MaxColorCount)
                throw new ArgumentOutOfRangeException(nameof(colorCount));
            if (random == null)
                throw new ArgumentNullException(nameof(random));

            var filled = new List<CellPos>();
            for (int y = 0; y < grid.Height; y++)
            {
                for (int x = 0; x < grid.Width; x++)
                {
                    if (grid.Get(x, y).IsEmpty) filled.Add(new CellPos(x, y));
                }
            }

            // Draw and check every value first, so a bad value leaves the grid untouched.
            var values = new int[filled.Count];
            for (int i = 0; i < values.Length; i++)
            {
                int value = random.Next(colorCount);
                if (value < 0 || value >= colorCount)
                    throw new InvalidOperationException(
                        "Random source returned " + value + ", expected 0.." + (colorCount - 1) + ".");
                values[i] = value;
            }

            for (int i = 0; i < values.Length; i++)
                grid.Set(filled[i].X, filled[i].Y, Cell.OfColor((TileColor)values[i]));
            return filled;
        }
    }
}
