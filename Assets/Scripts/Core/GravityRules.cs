using System;
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
            if (grid == null)
                throw new ArgumentNullException(nameof(grid));

            var moves = new List<CellMove>();
            for (int x = 0; x < grid.Width; x++)
            {
                int target = 0; // next free row from the bottom
                for (int y = 0; y < grid.Height; y++)
                {
                    Cell cell = grid.Get(x, y);
                    if (cell.IsEmpty) continue;
                    if (y != target)
                    {
                        grid.Set(x, target, cell);
                        grid.Set(x, y, Cell.Empty);
                        moves.Add(new CellMove(new CellPos(x, y), new CellPos(x, target)));
                    }
                    target++;
                }
            }
            return moves;
        }
    }
}
