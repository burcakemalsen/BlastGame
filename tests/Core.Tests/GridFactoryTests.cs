using System;
using System.Collections.Generic;
using Xunit;

namespace BlastGame.Core.Tests
{
    public class GridFactoryTests
    {
        [Theory]
        [InlineData(3, 3)]
        [InlineData(3, 4)]
        [InlineData(3, int.MaxValue)]
        [InlineData(3, -1)]
        [InlineData(3, int.MinValue)]
        [InlineData(1, 1)]
        [InlineData(1, -1)]
        [InlineData(6, 6)]
        [InlineData(6, 7)]
        [InlineData(6, -1)]
        public void CreateRandom_SourceValueOutOfRange_ThrowsInvalidOperation(int colorCount, int badValue)
        {
            var source = new SequenceRandomSource(badValue);

            Assert.Throws<InvalidOperationException>(() => GridFactory.CreateRandom(3, 2, colorCount, source));
        }

        [Theory]
        [InlineData(3)]
        [InlineData(-1)]
        public void CreateRandom_BadValueOnlyOnFirstCell_ThrowsInvalidOperation(int badValue)
        {
            var source = new SequenceRandomSource(badValue, 0, 1, 2, 0, 1);

            Assert.Throws<InvalidOperationException>(() => GridFactory.CreateRandom(3, 2, 3, source));
        }

        [Theory]
        [InlineData(3)]
        [InlineData(-1)]
        public void CreateRandom_BadValueOnlyOnLastCell_ThrowsInvalidOperation(int badValue)
        {
            var source = new SequenceRandomSource(0, 1, 2, 0, 1, badValue);

            Assert.Throws<InvalidOperationException>(() => GridFactory.CreateRandom(3, 2, 3, source));
        }

        [Fact]
        public void CreateRandom_SourceValuesAtRangeEdges_AreAccepted()
        {
            var source = new SequenceRandomSource(0, 2, 0, 2, 0, 2);

            Grid grid = GridFactory.CreateRandom(3, 2, 3, source);

            Assert.Equal(Cell.OfColor(TileColor.Red), grid.Get(0, 0));
            Assert.Equal(Cell.OfColor(TileColor.Blue), grid.Get(1, 0));
            Assert.Equal(Cell.OfColor(TileColor.Blue), grid.Get(2, 1));
        }

        [Fact]
        public void MaxColorCount_MatchesSpec_Six()
        {
            Assert.Equal(6, GridFactory.MaxColorCount);
        }

        // ---- Spec example and fill order ----

        [Fact]
        public void CreateRandom_3x2WithSequence012012_BottomRowThenTopRow()
        {
            var source = new SequenceRandomSource(0, 1, 2, 0, 1, 2);

            Grid grid = GridFactory.CreateRandom(3, 2, 3, source);

            Assert.Equal(Cell.OfColor(TileColor.Red), grid.Get(0, 0));
            Assert.Equal(Cell.OfColor(TileColor.Green), grid.Get(1, 0));
            Assert.Equal(Cell.OfColor(TileColor.Blue), grid.Get(2, 0));
            Assert.Equal(Cell.OfColor(TileColor.Red), grid.Get(0, 1));
            Assert.Equal(Cell.OfColor(TileColor.Green), grid.Get(1, 1));
            Assert.Equal(Cell.OfColor(TileColor.Blue), grid.Get(2, 1));
        }

        [Theory]
        [InlineData(4, 3)]
        [InlineData(3, 4)]
        [InlineData(5, 2)]
        [InlineData(2, 5)]
        [InlineData(16, 16)]
        [InlineData(16, 1)]
        [InlineData(1, 16)]
        public void CreateRandom_NonPeriodicSequence_FillsRowsFromBottomAndXLeftToRight(int width, int height)
        {
            int[] values = NonPeriodicValues(width * height, 6);
            var source = new SequenceRandomSource(values);

            Grid grid = GridFactory.CreateRandom(width, height, 6, source);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int index = y * width + x;
                    Assert.Equal(Cell.OfColor((TileColor)values[index]), grid.Get(x, y));
                }
            }
        }

        [Fact]
        public void CreateRandom_1x1_UsesFirstValue()
        {
            var source = new SequenceRandomSource(4);

            Grid grid = GridFactory.CreateRandom(1, 1, 6, source);

            Assert.Equal(Cell.OfColor(TileColor.Purple), grid.Get(0, 0));
        }

        [Theory]
        [InlineData(0, TileColor.Red)]
        [InlineData(1, TileColor.Green)]
        [InlineData(2, TileColor.Blue)]
        [InlineData(3, TileColor.Yellow)]
        [InlineData(4, TileColor.Purple)]
        [InlineData(5, TileColor.Orange)]
        public void CreateRandom_SourceValue_MapsToColorWithSameNumber(int value, TileColor expected)
        {
            var source = new SequenceRandomSource(value);

            Grid grid = GridFactory.CreateRandom(2, 2, 6, source);

            Assert.Equal(Cell.OfColor(expected), grid.Get(0, 0));
            Assert.Equal(Cell.OfColor(expected), grid.Get(1, 0));
            Assert.Equal(Cell.OfColor(expected), grid.Get(0, 1));
            Assert.Equal(Cell.OfColor(expected), grid.Get(1, 1));
        }

        // ---- Calls to the source ----

        [Theory]
        [InlineData(3, 2, 3)]
        [InlineData(1, 1, 1)]
        [InlineData(1, 1, 6)]
        [InlineData(16, 16, 6)]
        [InlineData(8, 10, 5)]
        [InlineData(16, 1, 4)]
        [InlineData(1, 16, 2)]
        public void CreateRandom_CallsNextExactlyOncePerCellWithColorCount(int width, int height, int colorCount)
        {
            var source = new SequenceRandomSource(0);

            GridFactory.CreateRandom(width, height, colorCount, source);

            Assert.Equal(width * height, source.Calls.Count);
            Assert.All(source.Calls, max => Assert.Equal(colorCount, max));
        }

        // ---- Result shape ----

        [Theory]
        [InlineData(3, 2)]
        [InlineData(2, 3)]
        [InlineData(1, 1)]
        [InlineData(16, 16)]
        [InlineData(16, 1)]
        [InlineData(1, 16)]
        [InlineData(8, 10)]
        public void CreateRandom_ValidSize_ReturnsGridWithThatWidthAndHeight(int width, int height)
        {
            Grid grid = GridFactory.CreateRandom(width, height, 3, new SequenceRandomSource(0, 1, 2));

            Assert.NotNull(grid);
            Assert.Equal(width, grid.Width);
            Assert.Equal(height, grid.Height);
        }

        [Theory]
        [InlineData(1, 1, 1)]
        [InlineData(8, 10, 5)]
        [InlineData(16, 16, 6)]
        [InlineData(1, 16, 3)]
        [InlineData(16, 1, 2)]
        public void CreateRandom_SeededSource_NoEmptyCells(int width, int height, int colorCount)
        {
            Grid grid = GridFactory.CreateRandom(width, height, colorCount, new SeededRandom(42));

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    Assert.False(grid.Get(x, y).IsEmpty, $"Cell ({x},{y}) is empty");
                }
            }
        }

        [Fact]
        public void CreateRandom_ColorCountOne_WholeGridIsRed()
        {
            Grid grid = GridFactory.CreateRandom(8, 10, 1, new SeededRandom(42));

            for (int y = 0; y < 10; y++)
            {
                for (int x = 0; x < 8; x++)
                {
                    Assert.Equal(Cell.OfColor(TileColor.Red), grid.Get(x, y));
                }
            }
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        [InlineData(5)]
        [InlineData(6)]
        public void CreateRandom_SeededSource16x16_OnlyFirstColorCountColorsAppear(int colorCount)
        {
            Grid grid = GridFactory.CreateRandom(16, 16, colorCount, new SeededRandom(42));

            for (int y = 0; y < 16; y++)
            {
                for (int x = 0; x < 16; x++)
                {
                    Cell cell = grid.Get(x, y);
                    Assert.Equal(CellKind.Color, cell.Kind);
                    TileColor color = cell.Color;
                    int value = (int)color;
                    Assert.True(value >= 0 && value < colorCount,
                        $"Cell ({x},{y}) has {color}, expected one of the first {colorCount} colors");
                }
            }
        }

        // ---- Determinism ----

        [Fact]
        public void CreateRandom_SameSeedTwice_GridsAreCellByCellEqual()
        {
            Grid a = GridFactory.CreateRandom(8, 10, 5, new SeededRandom(42));
            Grid b = GridFactory.CreateRandom(8, 10, 5, new SeededRandom(42));

            Assert.Equal(a.Width, b.Width);
            Assert.Equal(a.Height, b.Height);
            for (int y = 0; y < 10; y++)
            {
                for (int x = 0; x < 8; x++)
                {
                    Assert.Equal(a.Get(x, y), b.Get(x, y));
                }
            }
        }

        [Fact]
        public void CreateRandom_SeededSource_MatchesDirectDrawsInFillOrder()
        {
            Grid grid = GridFactory.CreateRandom(8, 10, 5, new SeededRandom(42));
            var reference = new SeededRandom(42);

            for (int y = 0; y < 10; y++)
            {
                for (int x = 0; x < 8; x++)
                {
                    Assert.Equal(Cell.OfColor((TileColor)reference.Next(5)), grid.Get(x, y));
                }
            }
        }

        [Fact]
        public void CreateRandom_TwoCalls_ReturnIndependentGrids()
        {
            Grid a = GridFactory.CreateRandom(3, 2, 3, new SequenceRandomSource(0));
            Grid b = GridFactory.CreateRandom(3, 2, 3, new SequenceRandomSource(0));

            a.Set(0, 0, Cell.OfColor(TileColor.Blue));

            Assert.Equal(Cell.OfColor(TileColor.Red), b.Get(0, 0));
        }

        // ---- Invalid input ----

        [Theory]
        [InlineData(0)]
        [InlineData(7)]
        [InlineData(-1)]
        [InlineData(int.MinValue)]
        [InlineData(int.MaxValue)]
        public void CreateRandom_ColorCountOutOfRange_ThrowsArgumentOutOfRange(int colorCount)
        {
            var source = new SequenceRandomSource(0);

            Assert.Throws<ArgumentOutOfRangeException>(
                () => GridFactory.CreateRandom(3, 2, colorCount, source));
        }

        [Theory]
        [InlineData(1)]
        [InlineData(6)]
        public void CreateRandom_ColorCountAtBounds_DoesNotThrow(int colorCount)
        {
            var source = new SequenceRandomSource(0);

            var ex = Record.Exception(() => GridFactory.CreateRandom(3, 2, colorCount, source));

            Assert.Null(ex);
        }

        [Fact]
        public void CreateRandom_NullRandom_ThrowsArgumentNull()
        {
            Assert.Throws<ArgumentNullException>(() => GridFactory.CreateRandom(3, 2, 3, null));
        }

        [Theory]
        [InlineData(0, 5)]
        [InlineData(17, 5)]
        [InlineData(-1, 5)]
        [InlineData(int.MinValue, 5)]
        [InlineData(5, 0)]
        [InlineData(5, 17)]
        [InlineData(5, -1)]
        [InlineData(5, int.MaxValue)]
        [InlineData(17, 16)]
        [InlineData(16, 17)]
        public void CreateRandom_SizeOutOfRange_ThrowsArgumentOutOfRange(int width, int height)
        {
            var source = new SequenceRandomSource(0);

            Assert.Throws<ArgumentOutOfRangeException>(
                () => GridFactory.CreateRandom(width, height, 3, source));
        }

        // ---- Helpers ----

        // Non-periodic values: for the multi-row, multi-column sizes above, filling
        // column-major, top-down or right-to-left gives a different grid than the
        // spec order. On 1-wide / 1-tall grids they still catch reversed order.
        private static int[] NonPeriodicValues(int count, int colorCount)
        {
            var values = new int[count];
            for (int i = 0; i < count; i++)
            {
                values[i] = (i * i + 3 * i + i / 7) % colorCount;
            }

            return values;
        }

        /// <summary>
        /// Test double: returns the given values in order (cycling when exhausted)
        /// and records the maxExclusive of every call.
        /// </summary>
        private sealed class SequenceRandomSource : IRandomSource
        {
            private readonly int[] _values;
            private int _index;

            public SequenceRandomSource(params int[] values)
            {
                if (values == null || values.Length == 0)
                {
                    throw new ArgumentException("At least one value is required.", nameof(values));
                }

                _values = values;
            }

            public List<int> Calls { get; } = new List<int>();

            public int Next(int maxExclusive)
            {
                Calls.Add(maxExclusive);
                int value = _values[_index % _values.Length];
                _index++;
                return value;
            }
        }
    }
}
