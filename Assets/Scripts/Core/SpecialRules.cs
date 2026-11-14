using System;
using System.Collections.Generic;

namespace BlastGame.Core
{
    /// <summary>Special item birth, activation and combos (with chains).</summary>
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

            return DrawRocket(random);
        }

        /// <summary>
        /// Tap on the special item at (x, y): a combo when 2+ connected special cells take part, otherwise the single effect;
        /// then the chain. Does not apply gravity or refill.
        /// </summary>
        public static IReadOnlyList<TurnEffect> Tap(Grid grid, int x, int y, IRandomSource random)
        {
            if (grid == null)
                throw new ArgumentNullException(nameof(grid));
            if (random == null)
                throw new ArgumentNullException(nameof(random));
            CheckSpecialCell(grid, x, y);

            List<CellPos> participants = FindParticipants(grid, x, y);
            if (participants.Count == 1)
                return Activate(grid, x, y);

            TileColor orbColor;
            EffectKind kind = PickCombo(grid, participants, out orbColor);

            // Conversion targets and Rocket directions are fixed before the grid changes.
            var converted = new List<ClearedCell>();
            Cell[] newItems = null;
            if (kind == EffectKind.OrbRocket || kind == EffectKind.OrbBomb)
            {
                Cell target = Cell.OfColor(orbColor);
                for (int cy = 0; cy < grid.Height; cy++)
                    for (int cx = 0; cx < grid.Width; cx++)
                        if (grid.Get(cx, cy) == target)
                            converted.Add(new ClearedCell(new CellPos(cx, cy), target));

                newItems = new Cell[converted.Count];
                for (int i = 0; i < newItems.Length; i++)
                    newItems[i] = kind == EffectKind.OrbBomb ? Cell.OfSpecial(SpecialKind.Bomb) : DrawRocket(random);
            }

            var effects = new List<TurnEffect>();
            var queued = new bool[grid.Width, grid.Height];
            var queue = new Queue<Pending>();
            var cleared = new List<ClearedCell>();

            foreach (CellPos p in participants)
            {
                cleared.Add(new ClearedCell(p, grid.Get(p.X, p.Y)));
                grid.Set(p.X, p.Y, Cell.Empty);
                queued[p.X, p.Y] = true;
            }

            switch (kind)
            {
                case EffectKind.RocketCross:
                    ScanCross(grid, x, y, 0, cleared, queued, queue);
                    break;
                case EffectKind.RocketBomb:
                    ScanCross(grid, x, y, 1, cleared, queued, queue);
                    break;
                case EffectKind.BigBomb:
                    for (int cy = Math.Max(0, y - 3); cy <= Math.Min(grid.Height - 1, y + 3); cy++)
                        for (int cx = Math.Max(0, x - 3); cx <= Math.Min(grid.Width - 1, x + 3); cx++)
                            Visit(grid, cx, cy, 0, cleared, queued, queue);
                    break;
                case EffectKind.OrbOrb:
                    // Every other filled cell is cleared; other special items are not triggered.
                    for (int cy = 0; cy < grid.Height; cy++)
                        for (int cx = 0; cx < grid.Width; cx++)
                        {
                            Cell cell = grid.Get(cx, cy);
                            if (cell.IsEmpty) continue;
                            cleared.Add(new ClearedCell(new CellPos(cx, cy), cell));
                            grid.Set(cx, cy, Cell.Empty);
                        }
                    break;
                default: // OrbRocket, OrbBomb: place the new items and queue them in order
                    for (int i = 0; i < converted.Count; i++)
                    {
                        CellPos p = converted[i].Pos;
                        grid.Set(p.X, p.Y, newItems[i]);
                        queued[p.X, p.Y] = true;
                        queue.Enqueue(new Pending(p, 0));
                    }
                    break;
            }

            effects.Add(new TurnEffect(kind, new CellPos(x, y), cleared, Cell.Empty, -1, participants.Count, converted));
            RunChain(grid, effects, queued, queue);
            return effects;
        }

        /// <summary>
        /// Runs the special item at (x, y), finishes the chain and returns the effects in run (queue) order.
        /// Does not apply gravity or refill.
        /// </summary>
        public static IReadOnlyList<TurnEffect> Activate(Grid grid, int x, int y)
        {
            if (grid == null)
                throw new ArgumentNullException(nameof(grid));
            CheckSpecialCell(grid, x, y);

            var effects = new List<TurnEffect>();
            var queued = new bool[grid.Width, grid.Height];
            var queue = new Queue<Pending>();
            queued[x, y] = true;
            queue.Enqueue(new Pending(new CellPos(x, y), -1));
            RunChain(grid, effects, queued, queue);
            return effects;
        }

        private static void CheckSpecialCell(Grid grid, int x, int y)
        {
            if (!grid.IsInside(x, y))
                throw new ArgumentOutOfRangeException(x < 0 || x >= grid.Width ? nameof(x) : nameof(y),
                    "Cell (" + x + ", " + y + ") is outside the grid.");
            if (!grid.Get(x, y).IsSpecial)
                throw new ArgumentException("Cell (" + x + ", " + y + ") is not a special item.");
        }

        private static Cell DrawRocket(IRandomSource random)
        {
            int draw = random.Next(2);
            switch (draw)
            {
                case 0: return Cell.OfSpecial(SpecialKind.RocketHorizontal);
                case 1: return Cell.OfSpecial(SpecialKind.RocketVertical);
                default:
                    throw new InvalidOperationException("Random source returned " + draw + " for Next(2).");
            }
        }

        /// <summary>Special cells connected to (x, y) through side neighbors: the tapped one first, then y, then x ascending.</summary>
        private static List<CellPos> FindParticipants(Grid grid, int x, int y)
        {
            var seen = new bool[grid.Width, grid.Height];
            var stack = new Stack<CellPos>();
            var others = new List<CellPos>();
            seen[x, y] = true;
            stack.Push(new CellPos(x, y));
            while (stack.Count > 0)
            {
                CellPos p = stack.Pop();
                for (int n = 0; n < 4; n++)
                {
                    int nx = p.X + (n == 0 ? -1 : n == 1 ? 1 : 0);
                    int ny = p.Y + (n == 2 ? -1 : n == 3 ? 1 : 0);
                    if (!grid.IsInside(nx, ny) || seen[nx, ny] || !grid.Get(nx, ny).IsSpecial) continue;
                    seen[nx, ny] = true;
                    var q = new CellPos(nx, ny);
                    others.Add(q);
                    stack.Push(q);
                }
            }

            others.Sort((a, b) => a.Y != b.Y ? a.Y.CompareTo(b.Y) : a.X.CompareTo(b.X));
            others.Insert(0, new CellPos(x, y));
            return others;
        }

        /// <summary>The strongest combo the participants can make, in the spec's power order.</summary>
        private static EffectKind PickCombo(Grid grid, List<CellPos> participants, out TileColor orbColor)
        {
            int orbs = 0, bombs = 0, rockets = 0;
            orbColor = default(TileColor);
            foreach (CellPos p in participants)
            {
                Cell cell = grid.Get(p.X, p.Y);
                switch (cell.Special)
                {
                    case SpecialKind.Orb:
                        orbColor = cell.OrbColor; // used only when there is exactly one Orb
                        orbs++;
                        break;
                    case SpecialKind.Bomb:
                        bombs++;
                        break;
                    default:
                        rockets++;
                        break;
                }
            }

            if (orbs >= 2) return EffectKind.OrbOrb;
            if (orbs == 1 && bombs > 0) return EffectKind.OrbBomb;
            if (orbs == 1 && rockets > 0) return EffectKind.OrbRocket;
            if (bombs >= 2) return EffectKind.BigBomb;
            if (bombs == 1 && rockets > 0) return EffectKind.RocketBomb;
            return EffectKind.RocketCross;
        }

        /// <summary>Rows oy-half..oy+half and columns ox-half..ox+half, in y then x order.</summary>
        private static void ScanCross(Grid grid, int ox, int oy, int half, List<ClearedCell> cleared,
                                      bool[,] queued, Queue<Pending> queue)
        {
            for (int cy = 0; cy < grid.Height; cy++)
                for (int cx = 0; cx < grid.Width; cx++)
                    if (Math.Abs(cy - oy) <= half || Math.Abs(cx - ox) <= half)
                        Visit(grid, cx, cy, 0, cleared, queued, queue);
        }

        /// <summary>Runs queued special items one by one (FIFO), each with its single effect (Step 6).</summary>
        private static void RunChain(Grid grid, List<TurnEffect> effects, bool[,] queued, Queue<Pending> queue)
        {
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
