using System.Collections.Generic;

namespace BlastGame.Core
{
    /// <summary>Rules for blasting a tapped group.</summary>
    public static class BlastRules
    {
        public const int MinGroupSize = 2;

        /// <summary>
        /// Empties the group at (x, y) if it has at least MinGroupSize cells and returns the emptied cells;
        /// otherwise leaves the grid alone and returns an empty list.
        /// </summary>
        public static IReadOnlyList<CellPos> Blast(Grid grid, int x, int y)
        {
            IReadOnlyList<CellPos> group = GroupFinder.FindGroup(grid, x, y);
            if (group.Count < MinGroupSize)
                return new List<CellPos>();

            foreach (CellPos p in group)
                grid.Set(p.X, p.Y, Cell.Empty);
            return group;
        }
    }
}
