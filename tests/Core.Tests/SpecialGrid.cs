using System;
using System.Collections.Generic;
using Xunit;

namespace BlastGame.Core.Tests
{
    /// <summary>
    /// Grid text helpers that also know special cells (top row first, last row is y = 0).
    /// Colors: R G B Y P O (enum order), '.' = empty.
    /// Specials: H = RocketHorizontal, V = RocketVertical, X = Bomb,
    /// lowercase r g b y p o = Orb of that color.
    /// Dump only touches special-cell members when the grid holds a special cell.
    /// </summary>
    internal static class SpecialGrid
    {
        private const string Colors = "RGBYPO";
        private const string Orbs = "rgbypo";

        public static Cell CellOf(char c)
        {
            if (c == '.')
            {
                return Cell.Empty;
            }

            int color = Colors.IndexOf(c);
            if (color >= 0)
            {
                return Cell.OfColor((TileColor)color);
            }

            int orb = Orbs.IndexOf(c);
            if (orb >= 0)
            {
                return Cell.OfOrb((TileColor)orb);
            }

            switch (c)
            {
                case 'H': return Cell.OfSpecial(SpecialKind.RocketHorizontal);
                case 'V': return Cell.OfSpecial(SpecialKind.RocketVertical);
                case 'X': return Cell.OfSpecial(SpecialKind.Bomb);
                default: throw new ArgumentException("Unknown cell letter '" + c + "'.", nameof(c));
            }
        }

        public static char LetterOf(Cell cell)
        {
            if (cell.IsEmpty)
            {
                return '.';
            }

            if (cell.Kind == CellKind.Color)
            {
                return Colors[(int)cell.Color];
            }

            switch (cell.Special)
            {
                case SpecialKind.RocketHorizontal: return 'H';
                case SpecialKind.RocketVertical: return 'V';
                case SpecialKind.Bomb: return 'X';
                default: return Orbs[(int)cell.OrbColor];
            }
        }

        public static Grid Parse(params string[] rows)
        {
            int height = rows.Length;
            int width = rows[0].Length;
            var grid = new Grid(width, height);
            for (int row = 0; row < height; row++)
            {
                if (rows[row].Length != width)
                {
                    throw new ArgumentException("All rows must have the same length.", nameof(rows));
                }

                for (int x = 0; x < width; x++)
                {
                    grid.Set(x, height - 1 - row, CellOf(rows[row][x]));
                }
            }

            return grid;
        }

        public static string[] Dump(Grid grid)
        {
            var rows = new string[grid.Height];
            for (int y = grid.Height - 1; y >= 0; y--)
            {
                var chars = new char[grid.Width];
                for (int x = 0; x < grid.Width; x++)
                {
                    chars[x] = LetterOf(grid.Get(x, y));
                }

                rows[grid.Height - 1 - y] = new string(chars);
            }

            return rows;
        }

        public static void AssertRows(string[] expectedRowsTopFirst, Grid grid)
        {
            Assert.Equal(expectedRowsTopFirst, Dump(grid));
        }

        /// <summary>Cell-by-cell comparison through Cell equality (not text).</summary>
        public static void AssertSameCells(Grid expected, Grid actual)
        {
            Assert.Equal(expected.Width, actual.Width);
            Assert.Equal(expected.Height, actual.Height);
            for (int y = 0; y < expected.Height; y++)
            {
                for (int x = 0; x < expected.Width; x++)
                {
                    Assert.True(expected.Get(x, y) == actual.Get(x, y),
                        "Cell (" + x + "," + y + ") changed: " + expected.Get(x, y) + " -> " + actual.Get(x, y));
                }
            }
        }

        /// <summary>Cleared list as "x,yL x,yL ..." in exact order, e.g. "2,1V 2,0G".</summary>
        public static string Format(IReadOnlyList<ClearedCell> cleared)
        {
            var parts = new List<string>();
            foreach (ClearedCell c in cleared)
            {
                parts.Add(c.Pos.X + "," + c.Pos.Y + LetterOf(c.Cell));
            }

            return string.Join(" ", parts);
        }

        /// <summary>One effect as text: "Kind x,y t=TriggeredBy c=CreatedLetter | cleared".</summary>
        public static string Format(TurnEffect e)
        {
            return e.Kind + " " + e.Origin.X + "," + e.Origin.Y + " t=" + e.TriggeredBy +
                   " c=" + LetterOf(e.Created) + " | " + Format(e.Cleared);
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

        /// <summary>Exact effects, in order. Each expected line uses the format of <see cref="Format(TurnEffect)"/>.</summary>
        public static void AssertEffects(IReadOnlyList<TurnEffect> actual, params string[] expected)
        {
            Assert.NotNull(actual);
            Assert.Equal(expected, Format(actual));
        }

        /// <summary>Every position appears in at most one Cleared list, and at most once.</summary>
        public static void AssertClearedUnique(IReadOnlyList<TurnEffect> effects)
        {
            var seen = new HashSet<(int, int)>();
            foreach (TurnEffect e in effects)
            {
                foreach (ClearedCell c in e.Cleared)
                {
                    Assert.True(seen.Add((c.Pos.X, c.Pos.Y)), "Cell cleared twice: " + c.Pos);
                }
            }
        }

        /// <summary>Concatenation of every effect's Cleared positions, in effect order.</summary>
        public static List<CellPos> ClearedPositions(IReadOnlyList<TurnEffect> effects)
        {
            var list = new List<CellPos>();
            foreach (TurnEffect e in effects)
            {
                foreach (ClearedCell c in e.Cleared)
                {
                    list.Add(c.Pos);
                }
            }

            return list;
        }
    }
}
