using System;
using System.Collections.Generic;
using Xunit;

namespace BlastGame.Core.Tests
{
    /// <summary>
    /// Grid text helpers for obstacle tests (spec 07). Rows top first (last row is y = 0),
    /// cells separated by spaces.
    /// Tokens: R G B Y U O = colors in enum order (U = Purple), lowercase r g b y u o = that color
    /// with a water balloon, P = Pinata, A = Anchor, C1 C2 C3 = Crate with that many layers,
    /// H = RocketHorizontal, V = RocketVertical, X = Bomb, *R *G ... = Orb of that color, '.' = empty.
    /// </summary>
    internal static class ObstacleGrid
    {
        private const string Colors = "RGBYUO";
        private const string Balloons = "rgbyuo";

        public static Cell CellOf(string token)
        {
            if (token == ".")
            {
                return Cell.Empty;
            }

            if (token.Length == 1)
            {
                int color = Colors.IndexOf(token[0]);
                if (color >= 0)
                {
                    return Cell.OfColor((TileColor)color);
                }

                int balloon = Balloons.IndexOf(token[0]);
                if (balloon >= 0)
                {
                    return Cell.OfColor((TileColor)balloon).WithBalloon(true);
                }

                switch (token[0])
                {
                    case 'P': return Cell.OfObstacle(ObstacleKind.Pinata, 1);
                    case 'A': return Cell.OfObstacle(ObstacleKind.Anchor, 1);
                    case 'H': return Cell.OfSpecial(SpecialKind.RocketHorizontal);
                    case 'V': return Cell.OfSpecial(SpecialKind.RocketVertical);
                    case 'X': return Cell.OfSpecial(SpecialKind.Bomb);
                }
            }

            if (token.Length == 2 && token[0] == 'C' && token[1] >= '1' && token[1] <= '3')
            {
                return Cell.OfObstacle(ObstacleKind.Crate, token[1] - '0');
            }

            if (token.Length == 2 && token[0] == '*' && Colors.IndexOf(token[1]) >= 0)
            {
                return Cell.OfOrb((TileColor)Colors.IndexOf(token[1]));
            }

            throw new ArgumentException("Unknown cell token '" + token + "'.", nameof(token));
        }

        public static string TokenOf(Cell cell)
        {
            switch (cell.Kind)
            {
                case CellKind.Empty:
                    return ".";
                case CellKind.Color:
                    return (cell.HasBalloon ? Balloons : Colors)[(int)cell.Color].ToString();
                case CellKind.Special:
                    switch (cell.Special)
                    {
                        case SpecialKind.RocketHorizontal: return "H";
                        case SpecialKind.RocketVertical: return "V";
                        case SpecialKind.Bomb: return "X";
                        default: return "*" + Colors[(int)cell.OrbColor];
                    }
                case CellKind.Obstacle:
                    switch (cell.Obstacle)
                    {
                        case ObstacleKind.Pinata: return "P";
                        case ObstacleKind.Anchor: return "A";
                        case ObstacleKind.Crate: return "C" + cell.Layers;
                        default: return "?" + cell.Obstacle;
                    }
                default:
                    return "?";
            }
        }

        public static Grid Parse(params string[] rows)
        {
            int height = rows.Length;
            string[] first = Split(rows[0]);
            int width = first.Length;
            var grid = new Grid(width, height);
            for (int row = 0; row < height; row++)
            {
                string[] tokens = Split(rows[row]);
                if (tokens.Length != width)
                {
                    throw new ArgumentException("All rows must have the same length.", nameof(rows));
                }

                for (int x = 0; x < width; x++)
                {
                    grid.Set(x, height - 1 - row, CellOf(tokens[x]));
                }
            }

            return grid;
        }

        public static string[] Dump(Grid grid)
        {
            var rows = new string[grid.Height];
            for (int y = grid.Height - 1; y >= 0; y--)
            {
                var tokens = new string[grid.Width];
                for (int x = 0; x < grid.Width; x++)
                {
                    tokens[x] = TokenOf(grid.Get(x, y));
                }

                rows[grid.Height - 1 - y] = string.Join(" ", tokens);
            }

            return rows;
        }

        public static void AssertRows(Grid grid, params string[] expectedRowsTopFirst)
        {
            Assert.Equal(expectedRowsTopFirst, Dump(grid));
        }

        /// <summary>One hit as "x,y Before>After", e.g. "1,1 C2>C1" or "1,1 P>.".</summary>
        public static string Format(ObstacleHit hit)
        {
            return hit.Pos.X + "," + hit.Pos.Y + " " + TokenOf(hit.Before) + ">" + TokenOf(hit.After);
        }

        public static string[] Format(IReadOnlyList<ObstacleHit> hits)
        {
            var list = new string[hits.Count];
            for (int i = 0; i < hits.Count; i++)
            {
                list[i] = Format(hits[i]);
            }

            return list;
        }

        /// <summary>Exact hits of one effect, in order.</summary>
        public static void AssertHits(TurnEffect effect, params string[] expected)
        {
            Assert.NotNull(effect.Hits);
            Assert.Equal(expected, Format(effect.Hits));
        }

        /// <summary>Hits of every effect, one array per effect, in effect order.</summary>
        public static string[][] HitsPerEffect(TurnResult result)
        {
            var list = new string[result.Effects.Count][];
            for (int i = 0; i < result.Effects.Count; i++)
            {
                list[i] = Format(result.Effects[i].Hits);
            }

            return list;
        }

        /// <summary>Cleared positions of every effect, joined in effect order, as "x,yT" tokens.</summary>
        public static string Cleared(TurnEffect effect)
        {
            var parts = new List<string>();
            foreach (ClearedCell c in effect.Cleared)
            {
                parts.Add(c.Pos.X + "," + c.Pos.Y + TokenOf(c.Cell));
            }

            return string.Join(" ", parts);
        }

        public static string Converted(TurnEffect effect)
        {
            var parts = new List<string>();
            foreach (ClearedCell c in effect.Converted)
            {
                parts.Add(c.Pos.X + "," + c.Pos.Y + TokenOf(c.Cell));
            }

            return string.Join(" ", parts);
        }

        /// <summary>Random source: the given values first, then zeros forever.</summary>
        public static CountingRandom ThenZeros(params int[] first)
        {
            var values = new int[first.Length + 64];
            Array.Copy(first, values, first.Length);
            return new CountingRandom(values);
        }

        private static string[] Split(string row)
        {
            return row.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        }
    }
}
