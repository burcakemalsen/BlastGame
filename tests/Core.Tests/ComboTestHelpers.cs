using System;
using System.Collections.Generic;
using Xunit;

namespace BlastGame.Core.Tests
{
    /// <summary>
    /// Helpers for combo tests (spec 06b). Effect text extends <see cref="SpecialGrid.Format(TurnEffect)"/>
    /// with MergedCount and Converted: "Kind x,y t=T c=C | cleared || m=M cv=converted".
    /// </summary>
    internal static class ComboHelpers
    {
        public static string Format(TurnEffect e)
        {
            return SpecialGrid.Format(e) + " || m=" + e.MergedCount + " cv=" + SpecialGrid.Format(e.Converted);
        }

        public static string[] Format(IReadOnlyList<TurnEffect> effects)
        {
            var list = new string[effects.Count];
            for (int i = 0; i < effects.Count; i++)
            {
                list[i] = Format(effects[i]);
            }

            return list;
        }

        /// <summary>Exact effects in order, each line in the format of <see cref="Format(TurnEffect)"/>.</summary>
        public static void AssertEffects(IReadOnlyList<TurnEffect> actual, params string[] expected)
        {
            Assert.NotNull(actual);
            Assert.Equal(expected, Format(actual));
        }

        /// <summary>"Kind x,y t=T" for each effect, to check chain order only.</summary>
        public static string[] Heads(IReadOnlyList<TurnEffect> effects)
        {
            var list = new string[effects.Count];
            for (int i = 0; i < effects.Count; i++)
            {
                TurnEffect e = effects[i];
                list[i] = e.Kind + " " + e.Origin.X + "," + e.Origin.Y + " t=" + e.TriggeredBy;
            }

            return list;
        }

        /// <summary>
        /// Board with a colored pattern from <paramref name="palette"/> (index (x + 2y) % length),
        /// then the given items placed as "x,y,L" entries (L a <see cref="SpecialGrid"/> letter).
        /// </summary>
        public static Grid Board(int width, int height, string palette, params string[] items)
        {
            Grid grid = TestGrid.Build(width, height,
                (x, y) => SpecialGrid.CellOf(palette[(x + 2 * y) % palette.Length]));
            foreach (string item in items)
            {
                string[] parts = item.Split(',');
                grid.Set(int.Parse(parts[0]), int.Parse(parts[1]), SpecialGrid.CellOf(parts[2][0]));
            }

            return grid;
        }

        /// <summary>
        /// Expected Cleared text for an area combo: participants first (in the given order), then every
        /// non-participant cell of the area, y ascending then x ascending, skipping empty cells.
        /// Throws when a non-participant special sits in the area (that one would be queued, not cleared).
        /// </summary>
        public static string AreaCleared(Grid before, CellPos[] participants, Func<int, int, bool> inArea)
        {
            var list = new List<ClearedCell>();
            var set = new HashSet<CellPos>(participants);
            foreach (CellPos p in participants)
            {
                list.Add(new ClearedCell(p, before.Get(p.X, p.Y)));
            }

            for (int y = 0; y < before.Height; y++)
            {
                for (int x = 0; x < before.Width; x++)
                {
                    var pos = new CellPos(x, y);
                    if (!inArea(x, y) || set.Contains(pos) || before.Get(x, y).IsEmpty)
                    {
                        continue;
                    }

                    if (before.Get(x, y).IsSpecial)
                    {
                        throw new InvalidOperationException("Test setup: special in area at " + pos);
                    }

                    list.Add(new ClearedCell(pos, before.Get(x, y)));
                }
            }

            return SpecialGrid.Format(list);
        }

        /// <summary>Every cell listed in a Cleared is empty now; every other cell is as before.</summary>
        public static void AssertOnlyClearedEmptied(Grid before, Grid after, IReadOnlyList<TurnEffect> effects)
        {
            var cleared = new HashSet<CellPos>(SpecialGrid.ClearedPositions(effects));
            for (int y = 0; y < before.Height; y++)
            {
                for (int x = 0; x < before.Width; x++)
                {
                    Cell expected = cleared.Contains(new CellPos(x, y)) ? Cell.Empty : before.Get(x, y);
                    Assert.True(expected == after.Get(x, y),
                        "Cell (" + x + "," + y + "): expected " + expected + " but was " + after.Get(x, y));
                }
            }
        }

        public static CellPos P(int x, int y)
        {
            return new CellPos(x, y);
        }
    }
}
