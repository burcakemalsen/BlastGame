using System;
using System.Collections.Generic;

namespace BlastGame.Core
{
    /// <summary>
    /// One tap: a group blast (maybe making a special item) or a special item chain, then gravity with Anchor
    /// collection and refill. A tap on an obstacle or an empty cell does nothing.
    /// </summary>
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
                effects = SpecialRules.Tap(grid, x, y, random);
            }
            else
            {
                // An obstacle or empty start gives an empty group.
                IReadOnlyList<CellPos> group = GroupFinder.FindGroup(grid, x, y);
                if (group.Count < BlastRules.MinGroupSize)
                    return new TurnResult(new List<CellPos>(), new List<CellMove>(), new List<CellPos>());

                // Draw the item (and a Rocket's direction) before the grid changes.
                Cell created = SpecialRules.ForGroup(group.Count, tapped.Color, random);

                var cleared = new List<ClearedCell>(group.Count);
                foreach (CellPos p in group)
                {
                    cleared.Add(new ClearedCell(p, grid.Get(p.X, p.Y))); // keeps the balloon, which pops
                    grid.Set(p.X, p.Y, Cell.Empty);
                }

                // One hit per neighboring Pinata or Crate, however many group cells touch it.
                var hits = new List<ObstacleHit>();
                SpecialRules.HitNeighbors(grid, cleared, new HashSet<CellPos>(), hits);

                if (!created.IsEmpty)
                    grid.Set(x, y, created);

                effects = new[]
                {
                    new TurnEffect(EffectKind.Group, new CellPos(x, y), cleared, created, -1, 0,
                                   new ClearedCell[0], SpecialRules.SortHits(hits))
                };
            }

            var blasted = new List<CellPos>();
            foreach (TurnEffect effect in effects)
                foreach (ClearedCell c in effect.Cleared)
                    blasted.Add(c.Pos);

            // Gravity (with Anchor collection) once, then refill once, after every effect.
            GravityResult gravity = GravityRules.Settle(grid);
            IReadOnlyList<CellPos> filled = RefillRules.Fill(grid, colorCount, random);
            return new TurnResult(blasted, gravity.Moves, filled, effects, gravity.Collected);
        }
    }
}
