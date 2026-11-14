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

        private static readonly IReadOnlyList<ClearedCell> NoCells = new ClearedCell[0];

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
                for (int cy = 0; cy < grid.Height; cy++)
                    for (int cx = 0; cx < grid.Width; cx++)
                    {
                        Cell cell = grid.Get(cx, cy);
                        if (cell.IsColor(orbColor)) // a balloon on it pops with the conversion
                            converted.Add(new ClearedCell(new CellPos(cx, cy), cell));
                    }

                newItems = new Cell[converted.Count];
                for (int i = 0; i < newItems.Length; i++)
                    newItems[i] = kind == EffectKind.OrbBomb ? Cell.OfSpecial(SpecialKind.Bomb) : DrawRocket(random);
            }

            var effects = new List<TurnEffect>();
            var queued = new bool[grid.Width, grid.Height];
            var queue = new Queue<Pending>();
            var cleared = new List<ClearedCell>();
            var hits = new List<ObstacleHit>();
            var hitSet = new HashSet<CellPos>(); // the combo is one blast: one hit per obstacle

            foreach (CellPos p in participants)
            {
                cleared.Add(new ClearedCell(p, grid.Get(p.X, p.Y)));
                grid.Set(p.X, p.Y, Cell.Empty);
                queued[p.X, p.Y] = true;
            }

            switch (kind)
            {
                case EffectKind.RocketCross:
                    ScanCross(grid, x, y, 0, cleared, hitSet, hits, queued, queue);
                    break;
                case EffectKind.RocketBomb:
                    ScanCross(grid, x, y, 1, cleared, hitSet, hits, queued, queue);
                    break;
                case EffectKind.BigBomb:
                    for (int cy = Math.Max(0, y - 3); cy <= Math.Min(grid.Height - 1, y + 3); cy++)
                        for (int cx = Math.Max(0, x - 3); cx <= Math.Min(grid.Width - 1, x + 3); cx++)
                            Visit(grid, cx, cy, 0, cleared, hitSet, hits, queued, queue);
                    break;
                case EffectKind.OrbOrb:
                    // Every other filled cell is cleared, every Pinata and Crate takes one hit, Anchors stay;
                    // other special items are not triggered.
                    for (int cy = 0; cy < grid.Height; cy++)
                        for (int cx = 0; cx < grid.Width; cx++)
                        {
                            Cell cell = grid.Get(cx, cy);
                            if (cell.IsEmpty) continue;
                            if (cell.IsObstacle)
                            {
                                Hit(grid, cx, cy, hitSet, hits);
                                continue;
                            }
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
                        queue.Enqueue(new Pending(p, 0, hitSet)); // the new items share the combo's blast
                    }
                    break;
            }

            effects.Add(new TurnEffect(kind, new CellPos(x, y), cleared, Cell.Empty, -1, participants.Count, converted,
                                       SortHits(hits)));
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
            queue.Enqueue(new Pending(new CellPos(x, y), -1, null));
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
                                      HashSet<CellPos> hitSet, List<ObstacleHit> hits, bool[,] queued, Queue<Pending> queue)
        {
            for (int cy = 0; cy < grid.Height; cy++)
                for (int cx = 0; cx < grid.Width; cx++)
                    if (Math.Abs(cy - oy) <= half || Math.Abs(cx - ox) <= half)
                        Visit(grid, cx, cy, 0, cleared, hitSet, hits, queued, queue);
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
                var hits = new List<ObstacleHit>();
                HashSet<CellPos> hitSet = item.HitSet ?? new HashSet<CellPos>(); // an item on its own is its own blast
                cleared.Add(new ClearedCell(o, self));
                grid.Set(o.X, o.Y, Cell.Empty);

                EffectKind kind;
                switch (self.Special)
                {
                    case SpecialKind.RocketHorizontal:
                        kind = EffectKind.Rocket;
                        for (int d = 1; d < grid.Width || d < grid.Height; d++)
                        {
                            Visit(grid, o.X - d, o.Y, index, cleared, hitSet, hits, queued, queue);
                            Visit(grid, o.X + d, o.Y, index, cleared, hitSet, hits, queued, queue);
                        }
                        break;
                    case SpecialKind.RocketVertical:
                        kind = EffectKind.Rocket;
                        for (int d = 1; d < grid.Width || d < grid.Height; d++)
                        {
                            Visit(grid, o.X, o.Y - d, index, cleared, hitSet, hits, queued, queue);
                            Visit(grid, o.X, o.Y + d, index, cleared, hitSet, hits, queued, queue);
                        }
                        break;
                    case SpecialKind.Bomb:
                        kind = EffectKind.Bomb;
                        for (int dy = -1; dy <= 1; dy++)
                            for (int dx = -1; dx <= 1; dx++)
                                if (dx != 0 || dy != 0)
                                    Visit(grid, o.X + dx, o.Y + dy, index, cleared, hitSet, hits, queued, queue);
                        break;
                    default: // Orb: only colored cells of its color, so it never starts a chain
                        kind = EffectKind.Orb;
                        TileColor orbColor = self.OrbColor;
                        for (int cy = 0; cy < grid.Height; cy++)
                            for (int cx = 0; cx < grid.Width; cx++)
                            {
                                Cell cell = grid.Get(cx, cy);
                                if (!cell.IsColor(orbColor)) continue;
                                cleared.Add(new ClearedCell(new CellPos(cx, cy), cell));
                                grid.Set(cx, cy, Cell.Empty);
                            }
                        // Side neighbors of every cleared cell (the Orb's own cell included) take a hit.
                        HitNeighbors(grid, cleared, hitSet, hits);
                        break;
                }

                effects.Add(new TurnEffect(kind, o, cleared, Cell.Empty, item.TriggeredBy, 1, NoCells, SortHits(hits)));
            }
        }

        private static void Visit(Grid grid, int x, int y, int index, List<ClearedCell> cleared,
                                  HashSet<CellPos> hitSet, List<ObstacleHit> hits, bool[,] queued, Queue<Pending> queue)
        {
            if (!grid.IsInside(x, y)) return;
            Cell cell = grid.Get(x, y);
            if (cell.IsEmpty) return;
            if (cell.IsObstacle)
            {
                Hit(grid, x, y, hitSet, hits); // a Rocket goes on through it
                return;
            }
            if (cell.IsSpecial)
            {
                if (queued[x, y]) return;
                queued[x, y] = true;
                queue.Enqueue(new Pending(new CellPos(x, y), index, null));
                return;
            }
            cleared.Add(new ClearedCell(new CellPos(x, y), cell));
            grid.Set(x, y, Cell.Empty);
        }

        /// <summary>
        /// One hit on the Pinata or Crate at (x, y), unless this blast already hit it; Anchors and other cells
        /// are left alone. Applies the hit to the grid and records it.
        /// </summary>
        internal static void Hit(Grid grid, int x, int y, HashSet<CellPos> hitSet, List<ObstacleHit> hits)
        {
            Cell before = grid.Get(x, y);
            if (!before.IsObstacle) return;
            ObstacleKind kind = before.Obstacle;
            if (kind != ObstacleKind.Pinata && kind != ObstacleKind.Crate) return;
            var pos = new CellPos(x, y);
            if (!hitSet.Add(pos)) return;

            Cell after = kind == ObstacleKind.Crate && before.Layers > 1
                ? Cell.OfObstacle(ObstacleKind.Crate, before.Layers - 1)
                : Cell.Empty;
            grid.Set(x, y, after);
            hits.Add(new ObstacleHit(pos, before, after));
        }

        /// <summary>Hits every Pinata and Crate that is a side neighbor of a cleared cell (once per blast).</summary>
        internal static void HitNeighbors(Grid grid, IReadOnlyList<ClearedCell> cleared, HashSet<CellPos> hitSet,
                                          List<ObstacleHit> hits)
        {
            foreach (ClearedCell c in cleared)
            {
                int cx = c.Pos.X, cy = c.Pos.Y;
                if (grid.IsInside(cx - 1, cy)) Hit(grid, cx - 1, cy, hitSet, hits);
                if (grid.IsInside(cx + 1, cy)) Hit(grid, cx + 1, cy, hitSet, hits);
                if (grid.IsInside(cx, cy - 1)) Hit(grid, cx, cy - 1, hitSet, hits);
                if (grid.IsInside(cx, cy + 1)) Hit(grid, cx, cy + 1, hitSet, hits);
            }
        }

        /// <summary>Sorts hits y ascending, then x ascending, and returns the same list.</summary>
        internal static List<ObstacleHit> SortHits(List<ObstacleHit> hits)
        {
            hits.Sort((a, b) => a.Pos.Y != b.Pos.Y ? a.Pos.Y.CompareTo(b.Pos.Y) : a.Pos.X.CompareTo(b.Pos.X));
            return hits;
        }

        private struct Pending
        {
            public readonly CellPos Pos;
            public readonly int TriggeredBy;
            public readonly HashSet<CellPos> HitSet; // shared by the items of one blast; null: the item is its own blast

            public Pending(CellPos pos, int triggeredBy, HashSet<CellPos> hitSet)
            {
                Pos = pos;
                TriggeredBy = triggeredBy;
                HitSet = hitSet;
            }
        }
    }
}
