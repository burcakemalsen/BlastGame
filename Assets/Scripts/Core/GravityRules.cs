using System.Collections.Generic;

namespace BlastGame.Core
{
    /// <summary>Rules for tiles falling down their columns.</summary>
    public static class GravityRules
    {
        /// <summary>
        /// Moves filled cells to the bottom of each column, keeping their order, and returns the moves
        /// (x ascending, then To.Y ascending). Cells that stay in place are not listed.
        /// </summary>
        public static IReadOnlyList<CellMove> Collapse(Grid grid)
        {
            throw new System.NotImplementedException();
        }
    }
}
