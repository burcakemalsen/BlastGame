using System.Collections.Generic;
using Xunit;

namespace BlastGame.Core.Tests
{
    /// <summary>
    /// Fake random source: returns the given values in order (or one value
    /// forever), counts calls and records every maxExclusive argument.
    /// Running out of values fails the test instead of returning junk.
    /// </summary>
    internal sealed class CountingRandom : IRandomSource
    {
        private readonly int[] _values;
        private readonly bool _repeatLast;
        private int _index;

        public CountingRandom(params int[] values)
        {
            _values = values;
        }

        private CountingRandom(int value, bool repeat)
        {
            _values = new[] { value };
            _repeatLast = repeat;
        }

        public static CountingRandom Constant(int value)
        {
            return new CountingRandom(value, true);
        }

        public static CountingRandom Cycle(int count, int length)
        {
            var values = new int[length];
            for (int i = 0; i < length; i++)
            {
                values[i] = i % count;
            }

            return new CountingRandom(values);
        }

        public int Calls { get; private set; }

        public List<int> MaxArgs { get; } = new List<int>();

        public int Next(int maxExclusive)
        {
            Calls++;
            MaxArgs.Add(maxExclusive);
            if (_index < _values.Length)
            {
                return _values[_index++];
            }

            if (_repeatLast)
            {
                return _values[_values.Length - 1];
            }

            Assert.Fail("CountingRandom ran out of values after " + _values.Length + " calls.");
            return 0;
        }
    }

    internal static class TurnTestHelpers
    {
        public static Grid Clone(Grid grid)
        {
            return TestGrid.Build(grid.Width, grid.Height, (x, y) => grid.Get(x, y));
        }

        /// <summary>Flat list fromX,fromY,toX,toY,... in exact order. Compares coordinates only.</summary>
        public static void AssertMoves(int[] expected, IReadOnlyList<CellMove> actual)
        {
            Assert.NotNull(actual);
            Assert.Equal(FormatQuads(expected), Format(actual));
        }

        public static string Format(IReadOnlyList<CellMove> moves)
        {
            var parts = new List<string>();
            foreach (CellMove m in moves)
            {
                parts.Add("(" + m.From.X + "," + m.From.Y + ")>(" + m.To.X + "," + m.To.Y + ")");
            }

            return "[" + string.Join(" ", parts) + "]";
        }

        public static string Format(IReadOnlyList<CellPos> cells)
        {
            var parts = new List<string>();
            foreach (CellPos p in cells)
            {
                parts.Add("(" + p.X + "," + p.Y + ")");
            }

            return "[" + string.Join(" ", parts) + "]";
        }

        private static string FormatQuads(int[] q)
        {
            var parts = new List<string>();
            for (int i = 0; i + 3 < q.Length; i += 4)
            {
                parts.Add("(" + q[i] + "," + q[i + 1] + ")>(" + q[i + 2] + "," + q[i + 3] + ")");
            }

            return "[" + string.Join(" ", parts) + "]";
        }
    }
}
