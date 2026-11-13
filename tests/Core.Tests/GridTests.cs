using System;
using Xunit;

namespace BlastGame.Core.Tests
{
    public class GridTests
    {
        // ---- Constants ----

        [Fact]
        public void SizeConstants_MatchSpec_MinOneMaxSixteen()
        {
            Assert.Equal(1, Grid.MinSize);
            Assert.Equal(16, Grid.MaxSize);
        }

        // ---- Constructor: valid sizes ----

        [Theory]
        [InlineData(1, 1)]
        [InlineData(16, 16)]
        [InlineData(1, 16)]
        [InlineData(16, 1)]
        [InlineData(3, 2)]
        [InlineData(2, 3)]
        [InlineData(8, 10)]
        public void Constructor_ValidSize_SetsWidthAndHeight(int width, int height)
        {
            var grid = new Grid(width, height);

            Assert.Equal(width, grid.Width);
            Assert.Equal(height, grid.Height);
        }

        // ---- Constructor: invalid sizes (width and height separately) ----

        [Theory]
        [InlineData(0)]
        [InlineData(17)]
        [InlineData(-1)]
        [InlineData(int.MinValue)]
        [InlineData(int.MaxValue)]
        public void Constructor_WidthOutOfRange_ThrowsArgumentOutOfRange(int width)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new Grid(width, 5));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(17)]
        [InlineData(-1)]
        [InlineData(int.MinValue)]
        [InlineData(int.MaxValue)]
        public void Constructor_HeightOutOfRange_ThrowsArgumentOutOfRange(int height)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new Grid(5, height));
        }

        [Theory]
        [InlineData(17, 16)]
        [InlineData(16, 17)]
        [InlineData(0, 1)]
        [InlineData(1, 0)]
        [InlineData(0, 0)]
        [InlineData(17, 17)]
        public void Constructor_OneOrBothDimensionsJustOutside_ThrowsArgumentOutOfRange(int width, int height)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new Grid(width, height));
        }

        // ---- New grid is empty ----

        [Fact]
        public void Constructor_3x2_EveryCellIsEmpty()
        {
            var grid = new Grid(3, 2);

            for (int y = 0; y < 2; y++)
            {
                for (int x = 0; x < 3; x++)
                {
                    Assert.Null(grid.Get(x, y));
                }
            }
        }

        [Theory]
        [InlineData(1, 1)]
        [InlineData(16, 16)]
        [InlineData(1, 16)]
        [InlineData(16, 1)]
        [InlineData(5, 9)]
        public void Constructor_AnyValidSize_EveryCellIsEmpty(int width, int height)
        {
            var grid = new Grid(width, height);

            AssertAllEmpty(grid, width, height);
        }

        // ---- IsInside ----

        [Fact]
        public void IsInside_EveryCellOfNonSquareGrid_ReturnsTrue()
        {
            var grid = new Grid(3, 5);

            for (int y = 0; y < 5; y++)
            {
                for (int x = 0; x < 3; x++)
                {
                    Assert.True(grid.IsInside(x, y), $"({x},{y}) should be inside a 3x5 grid");
                }
            }
        }

        [Theory]
        [InlineData(0, 0)]
        [InlineData(2, 0)]
        [InlineData(0, 1)]
        [InlineData(2, 1)]
        public void IsInside_Corners3x2_ReturnsTrue(int x, int y)
        {
            Assert.True(new Grid(3, 2).IsInside(x, y));
        }

        [Theory]
        [InlineData(-1, 0)]
        [InlineData(0, -1)]
        [InlineData(-1, -1)]
        [InlineData(3, 0)]
        [InlineData(0, 2)]
        [InlineData(3, 1)]
        [InlineData(2, 2)]
        [InlineData(3, 2)]
        [InlineData(-1, 1)]
        [InlineData(2, -1)]
        [InlineData(1, 2)] // swapped (x, y) of the inside cell (2, 1)
        [InlineData(int.MinValue, 0)]
        [InlineData(0, int.MinValue)]
        [InlineData(int.MaxValue, 0)]
        [InlineData(0, int.MaxValue)]
        [InlineData(int.MaxValue, int.MaxValue)]
        [InlineData(int.MinValue, int.MinValue)]
        public void IsInside_Outside3x2_ReturnsFalseWithoutThrowing(int x, int y)
        {
            var grid = new Grid(3, 2);

            bool result = true;
            var ex = Record.Exception(() => result = grid.IsInside(x, y));

            Assert.Null(ex);
            Assert.False(result);
        }

        [Fact]
        public void IsInside_1x1_OnlyOriginInside()
        {
            var grid = new Grid(1, 1);

            Assert.True(grid.IsInside(0, 0));
            Assert.False(grid.IsInside(1, 0));
            Assert.False(grid.IsInside(0, 1));
            Assert.False(grid.IsInside(-1, 0));
            Assert.False(grid.IsInside(0, -1));
        }

        [Fact]
        public void IsInside_16x16_CornersInsideAndJustOutsideFalse()
        {
            var grid = new Grid(16, 16);

            Assert.True(grid.IsInside(0, 0));
            Assert.True(grid.IsInside(15, 0));
            Assert.True(grid.IsInside(0, 15));
            Assert.True(grid.IsInside(15, 15));
            Assert.False(grid.IsInside(16, 0));
            Assert.False(grid.IsInside(0, 16));
            Assert.False(grid.IsInside(16, 15));
            Assert.False(grid.IsInside(15, 16));
        }

        [Fact]
        public void IsInside_WideGrid_UsesWidthForXAndHeightForY()
        {
            var grid = new Grid(16, 1);

            Assert.True(grid.IsInside(15, 0));
            Assert.False(grid.IsInside(0, 1));
            Assert.False(grid.IsInside(16, 0));
        }

        [Fact]
        public void IsInside_TallGrid_UsesWidthForXAndHeightForY()
        {
            var grid = new Grid(1, 16);

            Assert.True(grid.IsInside(0, 15));
            Assert.False(grid.IsInside(1, 0));
            Assert.False(grid.IsInside(0, 16));
        }

        // ---- Get / Set out of range ----

        [Theory]
        [InlineData(3, 0)]
        [InlineData(-1, 0)]
        [InlineData(0, 2)]
        [InlineData(0, -1)]
        [InlineData(3, 1)]
        [InlineData(2, 2)]
        [InlineData(3, 2)]
        [InlineData(-1, -1)]
        [InlineData(1, 2)] // swapped (x, y) of the inside cell (2, 1)
        [InlineData(int.MinValue, 0)]
        [InlineData(0, int.MaxValue)]
        public void Get_Outside3x2_ThrowsArgumentOutOfRange(int x, int y)
        {
            var grid = new Grid(3, 2);

            Assert.Throws<ArgumentOutOfRangeException>(() => grid.Get(x, y));
        }

        [Theory]
        [InlineData(3, 0)]
        [InlineData(-1, 0)]
        [InlineData(0, 2)]
        [InlineData(0, -1)]
        [InlineData(3, 1)]
        [InlineData(2, 2)]
        [InlineData(3, 2)]
        [InlineData(-1, -1)]
        [InlineData(1, 2)]
        [InlineData(int.MinValue, 0)]
        [InlineData(0, int.MaxValue)]
        public void Set_ColorOutside3x2_ThrowsArgumentOutOfRange(int x, int y)
        {
            var grid = new Grid(3, 2);

            Assert.Throws<ArgumentOutOfRangeException>(() => grid.Set(x, y, TileColor.Red));
        }

        [Theory]
        [InlineData(3, 0)]
        [InlineData(-1, 0)]
        [InlineData(0, 2)]
        [InlineData(0, -1)]
        public void Set_NullOutside3x2_ThrowsArgumentOutOfRange(int x, int y)
        {
            var grid = new Grid(3, 2);

            Assert.Throws<ArgumentOutOfRangeException>(() => grid.Set(x, y, null));
        }

        [Fact]
        public void Get_1x1_NeighboursOfOriginThrow()
        {
            var grid = new Grid(1, 1);

            Assert.Throws<ArgumentOutOfRangeException>(() => grid.Get(1, 0));
            Assert.Throws<ArgumentOutOfRangeException>(() => grid.Get(0, 1));
            Assert.Throws<ArgumentOutOfRangeException>(() => grid.Get(-1, 0));
            Assert.Throws<ArgumentOutOfRangeException>(() => grid.Get(0, -1));
        }

        [Fact]
        public void Get_16x16_JustOutsideCornersThrow()
        {
            var grid = new Grid(16, 16);

            Assert.Throws<ArgumentOutOfRangeException>(() => grid.Get(16, 0));
            Assert.Throws<ArgumentOutOfRangeException>(() => grid.Get(0, 16));
            Assert.Throws<ArgumentOutOfRangeException>(() => grid.Get(16, 15));
            Assert.Throws<ArgumentOutOfRangeException>(() => grid.Get(15, 16));
            Assert.Throws<ArgumentOutOfRangeException>(() => grid.Get(-1, 15));
            Assert.Throws<ArgumentOutOfRangeException>(() => grid.Get(15, -1));
        }

        [Fact]
        public void Get_WideGrid_YBeyondHeightThrowsEvenIfBelowWidth()
        {
            var grid = new Grid(5, 2);

            Assert.Null(grid.Get(4, 0));
            Assert.Throws<ArgumentOutOfRangeException>(() => grid.Get(0, 4));
            Assert.Throws<ArgumentOutOfRangeException>(() => grid.Set(0, 4, TileColor.Red));
        }

        [Fact]
        public void Get_TallGrid_XBeyondWidthThrowsEvenIfBelowHeight()
        {
            var grid = new Grid(2, 5);

            Assert.Null(grid.Get(0, 4));
            Assert.Throws<ArgumentOutOfRangeException>(() => grid.Get(4, 0));
            Assert.Throws<ArgumentOutOfRangeException>(() => grid.Set(4, 0, TileColor.Red));
        }

        // ---- Set / Get round trip ----

        [Fact]
        public void Set_Blue_At2And1_GetReturnsBlue()
        {
            var grid = new Grid(3, 2);

            grid.Set(2, 1, TileColor.Blue);

            Assert.Equal(TileColor.Blue, grid.Get(2, 1));
        }

        [Fact]
        public void Set_OneCell_OtherCellsStayEmpty()
        {
            var grid = new Grid(3, 2);

            grid.Set(2, 1, TileColor.Blue);

            for (int y = 0; y < 2; y++)
            {
                for (int x = 0; x < 3; x++)
                {
                    if (x == 2 && y == 1)
                    {
                        continue;
                    }

                    Assert.Null(grid.Get(x, y));
                }
            }
        }

        [Fact]
        public void Set_Null_ClearsCell()
        {
            var grid = new Grid(3, 2);
            grid.Set(2, 1, TileColor.Blue);

            grid.Set(2, 1, null);

            Assert.Null(grid.Get(2, 1));
        }

        [Fact]
        public void Set_NullOnEmptyCell_StaysEmpty()
        {
            var grid = new Grid(3, 2);

            grid.Set(1, 1, null);

            AssertAllEmpty(grid, 3, 2);
        }

        [Fact]
        public void Set_Null_OtherCellsKeepTheirColors()
        {
            var grid = new Grid(3, 2);
            grid.Set(0, 0, TileColor.Red);
            grid.Set(1, 0, TileColor.Green);
            grid.Set(2, 1, TileColor.Blue);

            grid.Set(1, 0, null);

            Assert.Equal(TileColor.Red, grid.Get(0, 0));
            Assert.Null(grid.Get(1, 0));
            Assert.Equal(TileColor.Blue, grid.Get(2, 1));
        }

        [Fact]
        public void Set_SameCellTwice_LastValueWins()
        {
            var grid = new Grid(3, 2);

            grid.Set(1, 0, TileColor.Red);
            grid.Set(1, 0, TileColor.Orange);

            Assert.Equal(TileColor.Orange, grid.Get(1, 0));
        }

        [Fact]
        public void Set_ColorThenNullThenColor_ReturnsLastColor()
        {
            var grid = new Grid(3, 2);

            grid.Set(0, 1, TileColor.Yellow);
            grid.Set(0, 1, null);
            grid.Set(0, 1, TileColor.Purple);

            Assert.Equal(TileColor.Purple, grid.Get(0, 1));
        }

        [Theory]
        [InlineData(TileColor.Red)]
        [InlineData(TileColor.Green)]
        [InlineData(TileColor.Blue)]
        [InlineData(TileColor.Yellow)]
        [InlineData(TileColor.Purple)]
        [InlineData(TileColor.Orange)]
        public void Set_EachColor_GetReturnsSameColor(TileColor color)
        {
            var grid = new Grid(3, 2);

            grid.Set(1, 1, color);

            Assert.Equal(color, grid.Get(1, 1));
        }

        [Fact]
        public void Set_1x1_RoundTrips()
        {
            var grid = new Grid(1, 1);

            grid.Set(0, 0, TileColor.Green);

            Assert.Equal(TileColor.Green, grid.Get(0, 0));
        }

        [Theory]
        [InlineData(4, 3)]
        [InlineData(3, 4)]
        [InlineData(16, 16)]
        [InlineData(16, 1)]
        [InlineData(1, 16)]
        public void Set_FourCorners_EachKeepsItsOwnColorAndRestStaysEmpty(int width, int height)
        {
            var grid = new Grid(width, height);
            int right = width - 1;
            int top = height - 1;

            // Write the corners in an order where a later write could overwrite
            // an earlier one if two corners alias (e.g. on 1-wide or 1-tall grids
            // two "corners" are the same cell, so expected values are computed below).
            grid.Set(0, 0, TileColor.Red);
            grid.Set(right, 0, TileColor.Green);
            grid.Set(0, top, TileColor.Blue);
            grid.Set(right, top, TileColor.Yellow);

            var expected = new TileColor?[width, height];
            expected[0, 0] = TileColor.Red;
            expected[right, 0] = TileColor.Green;
            expected[0, top] = TileColor.Blue;
            expected[right, top] = TileColor.Yellow;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    Assert.Equal(expected[x, y], grid.Get(x, y));
                }
            }
        }

        [Theory]
        [InlineData(7, 3)]
        [InlineData(3, 7)]
        [InlineData(16, 16)]
        [InlineData(16, 2)]
        [InlineData(2, 16)]
        public void Set_EveryCellWithPositionPattern_ReadsBackSamePattern(int width, int height)
        {
            var grid = new Grid(width, height);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    grid.Set(x, y, PatternColor(x, y));
                }
            }

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    Assert.Equal(PatternColor(x, y), grid.Get(x, y));
                }
            }
        }

        [Fact]
        public void Set_WideGrid_XAndYAreNotSwapped()
        {
            var grid = new Grid(5, 2);

            grid.Set(1, 0, TileColor.Green);

            Assert.Equal(TileColor.Green, grid.Get(1, 0));
            Assert.Null(grid.Get(0, 1));
        }

        [Fact]
        public void Set_TallGrid_XAndYAreNotSwapped()
        {
            var grid = new Grid(2, 5);

            grid.Set(0, 1, TileColor.Purple);

            Assert.Equal(TileColor.Purple, grid.Get(0, 1));
            Assert.Null(grid.Get(1, 0));
        }

        [Fact]
        public void Set_DoesNotChangeWidthOrHeight()
        {
            var grid = new Grid(4, 3);

            grid.Set(3, 2, TileColor.Red);
            grid.Set(0, 0, null);

            Assert.Equal(4, grid.Width);
            Assert.Equal(3, grid.Height);
        }

        [Fact]
        public void Constructor_TwoGrids_DoNotShareCells()
        {
            var a = new Grid(3, 2);
            var b = new Grid(3, 2);

            a.Set(0, 0, TileColor.Red);

            Assert.Null(b.Get(0, 0));
        }

        // ---- Helpers ----

        // Distinct-looking pattern where swapping x and y or using the wrong
        // row stride gives a different color for most cells.
        private static TileColor? PatternColor(int x, int y)
        {
            int v = (x * 5 + y * 3 + x * y) % 7;
            return v == 6 ? (TileColor?)null : (TileColor)v;
        }

        private static void AssertAllEmpty(Grid grid, int width, int height)
        {
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    Assert.Null(grid.Get(x, y));
                }
            }
        }
    }
}
