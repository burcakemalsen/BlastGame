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
            throw new System.NotImplementedException();
        }
    }
}
