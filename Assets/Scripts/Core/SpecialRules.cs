using System;
using System.Collections.Generic;

namespace BlastGame.Core
{
    /// <summary>Special item birth and activation (with chains).</summary>
    public static class SpecialRules
    {
        public const int RocketMinGroup = 5;
        public const int BombMinGroup = 7;
        public const int OrbMinGroup = 9;

        /// <summary>
        /// The item born from a group of the given size and color; size below 5 gives Cell.Empty without
        /// calling the source. Sizes 5-6 draw once with Next(2).
        /// </summary>
        public static Cell ForGroup(int size, TileColor color, IRandomSource random)
        {
            if (random == null)
                throw new ArgumentNullException(nameof(random));
            if (size < 0)
                throw new ArgumentOutOfRangeException(nameof(size), "Group size cannot be negative.");
            if (!Enum.IsDefined(typeof(TileColor), color))
                throw new ArgumentOutOfRangeException(nameof(color), "Undefined tile color: " + (int)color + ".");

            if (size < RocketMinGroup)
                return Cell.Empty;
            if (size >= OrbMinGroup)
                return Cell.OfOrb(color);
            if (size >= BombMinGroup)
                return Cell.OfSpecial(SpecialKind.Bomb);

            int draw = random.Next(2);
            switch (draw)
            {
                case 0: return Cell.OfSpecial(SpecialKind.RocketHorizontal);
                case 1: return Cell.OfSpecial(SpecialKind.RocketVertical);
                default:
                    throw new InvalidOperationException("Random source returned " + draw + " for Next(2).");
            }
        }

        /// <summary>
        /// Tap on the special item at (x, y): a combo when 2+ connected special cells take part, otherwise the single effect;
        /// then the chain. Does not apply gravity or refill.
        /// </summary>
        public static IReadOnlyList<TurnEffect> Tap(Grid grid, int x, int y, IRandomSource random)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Runs the special item at (x, y), finishes the chain and returns the effects in run (queue) order.
        /// Does not apply gravity or refill.
        /// </summary>
        public static IReadOnlyList<TurnEffect> Activate(Grid grid, int x, int y)
        {
            if (grid == null)
                throw new ArgumentNullException(nameof(grid));
            if (!grid.IsInside(x, y))
                throw new ArgumentOutOfRangeException(x < 0 || x >= grid.Width ? nameof(x) : nameof(y),
                    "Cell (" + x + ", " + y + ") is outside the grid.");
            if (!grid.Get(x, y).IsSpecial)
                throw new ArgumentException("Cell (" + x + ", " + y + ") is not a special item.");

            var effects = new List<TurnEffect>();
            var queued = new bool[grid.Width, grid.Height];
            var queue = new Queue<Pending>();
            queued[x, y] = true;
            queue.Enqueue(new Pending(new CellPos(x, y), -1));

            while (queue.Count > 0)
            {
                Pending item = queue.Dequeue();
                int index = effects.Count;
                CellPos o = item.Pos;
                Cell self = grid.Get(o.X, o.Y);

                var cleared = new List<ClearedCell>();
                cleared.Add(new ClearedCell(o, self));
                grid.Set(o.X, o.Y, Cell.Empty);

                EffectKind kind;
                switch (self.Special)
                {
                    case SpecialKind.RocketHorizontal:
                        kind = EffectKind.Rocket;
                        for (int d = 1; d < grid.Width || d < grid.Height; d++)
                        {
                            Visit(grid, o.X - d, o.Y, index, cleared, queued, queue);
                            Visit(grid, o.X + d, o.Y, index, cleared, queued, queue);
                        }
                        break;
                    case SpecialKind.RocketVertical:
                        kind = EffectKind.Rocket;
                        for (int d = 1; d < grid.Width || d < grid.Height; d++)
                        {
                            Visit(grid, o.X, o.Y - d, index, cleared, queued, queue);
                            Visit(grid, o.X, o.Y + d, index, cleared, queued, queue);
                        }
                        break;
                    case SpecialKind.Bomb:
                        kind = EffectKind.Bomb;
                        for (int dy = -1; dy <= 1; dy++)
                            for (int dx = -1; dx <= 1; dx++)
                                if (dx != 0 || dy != 0)
                                    Visit(grid, o.X + dx, o.Y + dy, index, cleared, queued, queue);
                        break;
                    default: // Orb: only colored cells of its color, so it never starts a chain
                        kind = EffectKind.Orb;
                        Cell target = Cell.OfColor(self.OrbColor);
                        for (int cy = 0; cy < grid.Height; cy++)
                            for (int cx = 0; cx < grid.Width; cx++)
                                if (grid.Get(cx, cy) == target)
                                {
                                    cleared.Add(new ClearedCell(new CellPos(cx, cy), target));
                                    grid.Set(cx, cy, Cell.Empty);
                                }
                        break;
                }

                effects.Add(new TurnEffect(kind, o, cleared, Cell.Empty, item.TriggeredBy));
            }

            return effects;
        }

        private static void Visit(Grid grid, int x, int y, int index, List<ClearedCell> cleared, bool[,] queued,
                                  Queue<Pending> queue)
        {
            if (!grid.IsInside(x, y)) return;
            Cell cell = grid.Get(x, y);
            if (cell.IsEmpty) return;
            if (cell.IsSpecial)
            {
                if (queued[x, y]) return;
                queued[x, y] = true;
                queue.Enqueue(new Pending(new CellPos(x, y), index));
                return;
            }
            cleared.Add(new ClearedCell(new CellPos(x, y), cell));
            grid.Set(x, y, Cell.Empty);
        }

        private struct Pending
        {
            public readonly CellPos Pos;
            public readonly int TriggeredBy;

            public Pending(CellPos pos, int triggeredBy)
            {
                Pos = pos;
                TriggeredBy = triggeredBy;
            }
        }
    }
}
