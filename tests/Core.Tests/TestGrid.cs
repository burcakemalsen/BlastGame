using System;
using System.Collections.Generic;
using Xunit;

namespace BlastGame.Core.Tests
{
    /// <summary>
    /// Test helpers: build a grid from text rows (top row first, so the last
    /// row is y = 0), dump a grid back to text, and compare groups.
    /// Letters: R G B Y P O = the six colors in enum order, '.' = empty.
    /// </summary>
    internal static class TestGrid
    {
        private const string Letters = "RGBYPO";

        public static Grid Parse(params string[] rows)
        {
            if (rows == null || rows.Length == 0)
            {
                throw new ArgumentException("At least one row is required.", nameof(rows));
            }

            int height = rows.Length;
            int width = rows[0].Length;
            var grid = new Grid(width, height);

            for (int row = 0; row < height; row++)
            {
                if (rows[row].Length != width)
                {
                    throw new ArgumentException("All rows must have the same length.", nameof(rows));
                }

                int y = height - 1 - row;
                for (int x = 0; x < width; x++)
                {
                    char c = rows[row][x];
                    if (c == '.')
                    {
                        grid.Set(x, y, Cell.Empty);
                        continue;
                    }

                    int index = Letters.IndexOf(c);
                    if (index < 0)
                    {
                        throw new ArgumentException("Unknown cell letter '" + c + "'.", nameof(rows));
                    }

                    grid.Set(x, y, Cell.OfColor((TileColor)index));
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
                    Cell cell = grid.Get(x, y);
                    chars[x] = cell.IsEmpty ? '.' : Letters[(int)cell.Color];
                }

                rows[grid.Height - 1 - y] = new string(chars);
            }

            return rows;
        }

        public static Grid Build(int width, int height, Func<int, int, Cell> cellAt)
        {
            var grid = new Grid(width, height);
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    grid.Set(x, y, cellAt(x, y));
                }
            }

            return grid;
        }

        public static Grid Filled(int width, int height, TileColor color)
        {
            return Build(width, height, (x, y) => Cell.OfColor(color));
        }

        /// <summary>Compares the grid with text rows (top row first).</summary>
        public static void AssertRows(string[] expectedRowsTopFirst, Grid grid)
        {
            Assert.Equal(expectedRowsTopFirst, Dump(grid));
        }

        /// <summary>Flat list x0,y0,x1,y1,... for readable expectations.</summary>
        public static int[] Pts(params int[] xy)
        {
            if (xy.Length % 2 != 0)
            {
                throw new ArgumentException("Coordinates come in x,y pairs.", nameof(xy));
            }

            return xy;
        }

        /// <summary>Exact order, same length, no duplicates. Compares X and Y only.</summary>
        public static void AssertGroup(int[] expectedXy, IReadOnlyList<CellPos> actual)
        {
            Assert.NotNull(actual);
            string expectedText = Format(expectedXy);
            string actualText = Format(actual);
            Assert.True(expectedXy.Length / 2 == actual.Count,
                "Expected " + expectedText + " but got " + actualText);

            var seen = new HashSet<(int, int)>();
            for (int i = 0; i < actual.Count; i++)
            {
                Assert.True(expectedXy[2 * i] == actual[i].X && expectedXy[2 * i + 1] == actual[i].Y,
                    "Expected " + expectedText + " but got " + actualText);
                Assert.True(seen.Add((actual[i].X, actual[i].Y)),
                    "Duplicate cell in result: " + actualText);
            }
        }

        private static string Format(int[] xy)
        {
            var parts = new List<string>();
            for (int i = 0; i + 1 < xy.Length; i += 2)
            {
                parts.Add("(" + xy[i] + "," + xy[i + 1] + ")");
            }

            return "[" + string.Join(" ", parts) + "]";
        }

        private static string Format(IReadOnlyList<CellPos> cells)
        {
            var parts = new List<string>();
            foreach (CellPos p in cells)
            {
                parts.Add("(" + p.X + "," + p.Y + ")");
            }

            return "[" + string.Join(" ", parts) + "]";
        }
    }
}
