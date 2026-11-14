using System;
using System.Collections.Generic;

namespace BlastGame.Core
{
    /// <summary>Rules for tiles falling down their columns.</summary>
    public static class GravityRules
    {
        /// <summary>
        /// Moves filled cells to the bottom of their column section, keeping their order, and returns the moves
        /// (x ascending, then To.Y ascending). Crates stay fixed and split a column into sections; nothing falls
        /// past a Crate. Cells that stay in place are not listed. Anchors are not collected.
        /// </summary>
        public static IReadOnlyList<CellMove> Collapse(Grid grid)
        {
            if (grid == null)
                throw new ArgumentNullException(nameof(grid));

            var moves = new List<CellMove>();
            for (int x = 0; x < grid.Width; x++)
                SettleColumn(grid, x, false, moves, null);
            return moves;
        }

        /// <summary>
        /// Like <see cref="Collapse"/>, and in each column's bottom section the leading run of Anchors
        /// (the ones that land on y = 0 one after another) is collected: removed from the grid, with the cells
        /// above falling into their place.
        /// </summary>
        public static GravityResult Settle(Grid grid)
        {
            if (grid == null)
                throw new ArgumentNullException(nameof(grid));

            var moves = new List<CellMove>();
            var collected = new List<CellMove>();
            for (int x = 0; x < grid.Width; x++)
                SettleColumn(grid, x, true, moves, collected);
            return new GravityResult(moves, collected);
        }

        /// <summary>Settles one column; moves are added with To.Y ascending, collected with From.Y ascending.</summary>
        private static void SettleColumn(Grid grid, int x, bool collect, List<CellMove> moves, List<CellMove> collected)
        {
            int sectionStart = 0; // lowest row of the current section
            while (sectionStart < grid.Height)
            {
                int sectionEnd = sectionStart; // first Crate row above the section, or Height
                while (sectionEnd < grid.Height && !IsCrate(grid.Get(x, sectionEnd)))
                    sectionEnd++;

                int target = sectionStart; // next free row in the section
                bool collecting = collect && sectionStart == 0;
                for (int y = sectionStart; y < sectionEnd; y++)
                {
                    Cell cell = grid.Get(x, y);
                    if (cell.IsEmpty) continue;

                    if (collecting && cell.IsObstacle && cell.Obstacle == ObstacleKind.Anchor)
                    {
                        // This Anchor would land on y = 0: collect it instead of moving it.
                        grid.Set(x, y, Cell.Empty);
                        collected.Add(new CellMove(new CellPos(x, y), new CellPos(x, 0)));
                        continue;
                    }
                    collecting = false;

                    if (y != target)
                    {
                        grid.Set(x, target, cell);
                        grid.Set(x, y, Cell.Empty);
                        moves.Add(new CellMove(new CellPos(x, y), new CellPos(x, target)));
                    }
                    target++;
                }

                sectionStart = sectionEnd + 1; // skip the Crate
            }
        }

        private static bool IsCrate(Cell cell)
        {
            return cell.IsObstacle && cell.Obstacle == ObstacleKind.Crate;
        }
    }
}
