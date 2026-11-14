using System;
using System.Collections.Generic;

namespace BlastGame.Core
{
    /// <summary>One tap: a group blast (maybe making a special item) or a special item chain, then collapse and refill.</summary>
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

            Cell tapped = grid.Get(x, y);
            IReadOnlyList<TurnEffect> effects;
            if (tapped.IsSpecial)
            {
                effects = SpecialRules.Activate(grid, x, y);
            }
            else
            {
                IReadOnlyList<CellPos> group = GroupFinder.FindGroup(grid, x, y);
                if (group.Count < BlastRules.MinGroupSize)
                    return new TurnResult(new List<CellPos>(), new List<CellMove>(), new List<CellPos>());

                // Draw the item (and a Rocket's direction) before the grid changes.
                Cell created = SpecialRules.ForGroup(group.Count, tapped.Color, random);

                var cleared = new List<ClearedCell>(group.Count);
                foreach (CellPos p in group)
                {
                    cleared.Add(new ClearedCell(p, tapped));
                    grid.Set(p.X, p.Y, Cell.Empty);
                }
                if (!created.IsEmpty)
                    grid.Set(x, y, created);

                effects = new[] { new TurnEffect(EffectKind.Group, new CellPos(x, y), cleared, created, -1) };
            }

            var blasted = new List<CellPos>();
            foreach (TurnEffect effect in effects)
                foreach (ClearedCell c in effect.Cleared)
                    blasted.Add(c.Pos);

            // Gravity once, then refill once, after every effect.
            IReadOnlyList<CellMove> moves = GravityRules.Collapse(grid);
            IReadOnlyList<CellPos> filled = RefillRules.Fill(grid, colorCount, random);
            return new TurnResult(blasted, moves, filled, effects);
        }
    }
}
