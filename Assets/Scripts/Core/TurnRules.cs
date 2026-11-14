using System;
using System.Collections.Generic;

namespace BlastGame.Core
{
    /// <summary>One tap: blast, then collapse and refill if something blasted.</summary>
    public static class TurnRules
    {
        public static TurnResult Tap(Grid grid, int x, int y, int colorCount, IRandomSource random)
        {
            // Validate everything before changing the grid or touching the random source.
            if (grid == null)
                throw new ArgumentNullException(nameof(grid));
            if (random == null)
                throw new ArgumentNullException(nameof(random));
            if (colorCount < 1 || colorCount > GridFactory.MaxColorCount)
                throw new ArgumentOutOfRangeException(nameof(colorCount));
            if (x < 0 || x >= grid.Width)
                throw new ArgumentOutOfRangeException(nameof(x));
            if (y < 0 || y >= grid.Height)
                throw new ArgumentOutOfRangeException(nameof(y));

            IReadOnlyList<CellPos> blasted = BlastRules.Blast(grid, x, y);
            if (blasted.Count == 0)
                return new TurnResult(blasted, new List<CellMove>(), new List<CellPos>());

            IReadOnlyList<CellMove> moves = GravityRules.Collapse(grid);
            IReadOnlyList<CellPos> filled = RefillRules.Fill(grid, colorCount, random);
            return new TurnResult(blasted, moves, filled);
        }
    }
}
