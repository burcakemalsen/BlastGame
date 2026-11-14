using System;
using System.Collections.Generic;

namespace BlastGame.Core
{
    /// <summary>Rules for filling empty cells with random colors.</summary>
    public static class RefillRules
    {
        /// <summary>
        /// Fills every empty cell above the topmost Crate of its column (every empty cell when the column has
        /// no Crate), y ascending, then x ascending, with a color from the random source, and returns the
        /// filled cells in that order. Empty cells below a Crate stay empty and take no draw.
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

            // Lowest row each column may fill: just above its topmost Crate.
            var lowest = new int[grid.Width];
            for (int x = 0; x < grid.Width; x++)
                for (int y = grid.Height - 1; y >= 0; y--)
                {
                    Cell cell = grid.Get(x, y);
                    if (cell.IsObstacle && cell.Obstacle == ObstacleKind.Crate)
                    {
                        lowest[x] = y + 1;
                        break;
                    }
                }

            var filled = new List<CellPos>();
            for (int y = 0; y < grid.Height; y++)
            {
                for (int x = 0; x < grid.Width; x++)
                {
                    if (y >= lowest[x] && grid.Get(x, y).IsEmpty) filled.Add(new CellPos(x, y));
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
