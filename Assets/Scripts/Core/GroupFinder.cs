using System;
using System.Collections.Generic;

namespace BlastGame.Core
{
    /// <summary>Finds groups of same-colored cells.</summary>
    public static class GroupFinder
    {
        /// <summary>
        /// Cells connected to (x, y) through same-colored 4-neighbours, start included,
        /// ordered by y then x. Empty start cell gives an empty list.
        /// </summary>
        public static IReadOnlyList<CellPos> FindGroup(Grid grid, int x, int y)
        {
            if (grid == null)
                throw new ArgumentNullException(nameof(grid));
            if (!grid.IsInside(x, y))
                throw new ArgumentOutOfRangeException(x < 0 || x >= grid.Width ? nameof(x) : nameof(y),
                    "Cell (" + x + ", " + y + ") is outside the grid.");

            var result = new List<CellPos>();
            Cell start = grid.Get(x, y);
            if (start.IsEmpty) return result;

            var visited = new bool[grid.Width, grid.Height];
            var stack = new Stack<CellPos>();
            visited[x, y] = true;
            stack.Push(new CellPos(x, y));

            while (stack.Count > 0)
            {
                CellPos p = stack.Pop();
                TryVisit(grid, start, visited, stack, p.X, p.Y + 1);
                TryVisit(grid, start, visited, stack, p.X, p.Y - 1);
                TryVisit(grid, start, visited, stack, p.X - 1, p.Y);
                TryVisit(grid, start, visited, stack, p.X + 1, p.Y);
            }

            // Collect in the fixed order (y, then x).
            for (int cy = 0; cy < grid.Height; cy++)
                for (int cx = 0; cx < grid.Width; cx++)
                    if (visited[cx, cy])
                        result.Add(new CellPos(cx, cy));
            return result;
        }

        private static void TryVisit(Grid grid, Cell start, bool[,] visited, Stack<CellPos> stack, int x, int y)
        {
            if (!grid.IsInside(x, y) || visited[x, y]) return;
            if (grid.Get(x, y) != start) return;
            visited[x, y] = true;
            stack.Push(new CellPos(x, y));
        }
    }
}
