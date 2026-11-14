using System;
using System.Collections.Generic;
using Xunit;

namespace BlastGame.Core.Tests
{
    public class RefillRulesTests
    {
        [Fact]
        public void Fill_Example2_ExactCellsColorsAndCalls()
        {
            Grid grid = TestGrid.Parse("...", "..B", "G.B", "RRG");
            var random = new CountingRandom(0, 1, 2, 0, 1, 2);

            IReadOnlyList<CellPos> filled = RefillRules.Fill(grid, 3, random);

            TestGrid.AssertGroup(TestGrid.Pts(1, 1, 0, 2, 1, 2, 0, 3, 1, 3, 2, 3), filled);
            TestGrid.AssertRows(new[] { "RGB", "GBB", "GRB", "RRG" }, grid);
            Assert.Equal(6, random.Calls);
        }

        [Fact]
        public void Fill_NoEmptyCell_EmptyListSourceNeverCalled()
        {
            Grid grid = TestGrid.Parse("RG", "BY");
            var random = new CountingRandom();

            Assert.Empty(RefillRules.Fill(grid, 4, random));
            Assert.Equal(0, random.Calls);
            TestGrid.AssertRows(new[] { "RG", "BY" }, grid);
        }

        [Fact]
        public void Fill_SingleEmptyCell_OneCallOneCell()
        {
            Grid grid = TestGrid.Parse("RGB", "B.Y", "YGR");
            var random = new CountingRandom(4);

            TestGrid.AssertGroup(TestGrid.Pts(1, 1), RefillRules.Fill(grid, 5, random));
            Assert.Equal(1, random.Calls);
            TestGrid.AssertRows(new[] { "RGB", "BPY", "YGR" }, grid);
        }

        [Fact]
        public void Fill_Whole16x16Empty_256CallsRowMajorFromBottom()
        {
            var grid = new Grid(16, 16);
            var random = CountingRandom.Cycle(6, 256);

            IReadOnlyList<CellPos> filled = RefillRules.Fill(grid, 6, random);

            Assert.Equal(256, random.Calls);
            Assert.Equal(256, filled.Count);
            for (int i = 0; i < 256; i++)
            {
                int x = i % 16;
                int y = i / 16;
                Assert.Equal(new CellPos(x, y), filled[i]);
                Assert.Equal(Cell.OfColor((TileColor)(i % 6)), grid.Get(x, y));
            }
        }

        [Fact]
        public void Fill_MixedGrid_FilledCellsKeepTheirColors()
        {
            Grid grid = TestGrid.Parse(".O.", "P.Y", ".B.");
            var random = CountingRandom.Constant(0);

            TestGrid.AssertGroup(TestGrid.Pts(0, 0, 2, 0, 1, 1, 0, 2, 2, 2), RefillRules.Fill(grid, 6, random));
            TestGrid.AssertRows(new[] { "ROR", "PRY", "RBR" }, grid);
            Assert.Equal(5, random.Calls);
        }

        [Fact]
        public void Fill_GapBelowFilledCell_FilledInPlaceWithoutCollapsing()
        {
            Grid grid = TestGrid.Parse("G", ".", "R");
            var random = new CountingRandom(2);

            TestGrid.AssertGroup(TestGrid.Pts(0, 1), RefillRules.Fill(grid, 3, random));
            TestGrid.AssertRows(new[] { "G", "B", "R" }, grid);
        }

        [Fact]
        public void Fill_AnyColorCount_PassesColorCountAsMaxExclusive()
        {
            Grid grid = TestGrid.Parse("..", "..");
            var random = CountingRandom.Constant(1);

            RefillRules.Fill(grid, 5, random);

            Assert.Equal(new[] { 5, 5, 5, 5 }, random.MaxArgs);
        }

        [Fact]
        public void Fill_ColorCountOne_AllRed()
        {
            Grid grid = TestGrid.Parse("..", "..");
            var random = CountingRandom.Constant(0);

            RefillRules.Fill(grid, 1, random);

            TestGrid.AssertRows(new[] { "RR", "RR" }, grid);
            Assert.Equal(new[] { 1, 1, 1, 1 }, random.MaxArgs);
        }

        [Fact]
        public void Fill_ColorCountSixValueFive_Orange()
        {
            Grid grid = TestGrid.Parse(".");
            RefillRules.Fill(grid, GridFactory.MaxColorCount, new CountingRandom(5));
            TestGrid.AssertRows(new[] { "O" }, grid);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(7)]
        [InlineData(int.MinValue)]
        public void Fill_ColorCountOutOfRange_ThrowsSourceAndGridUntouched(int colorCount)
        {
            Grid grid = TestGrid.Parse("R.", ".G");
            var random = CountingRandom.Constant(0);

            Assert.Throws<ArgumentOutOfRangeException>(() => RefillRules.Fill(grid, colorCount, random));
            Assert.Equal(0, random.Calls);
            TestGrid.AssertRows(new[] { "R.", ".G" }, grid);
        }

        [Fact]
        public void Fill_NullGrid_ThrowsArgumentNull()
        {
            var random = CountingRandom.Constant(0);
            Assert.Throws<ArgumentNullException>(() => RefillRules.Fill(null, 3, random));
            Assert.Equal(0, random.Calls);
        }

        [Fact]
        public void Fill_NullRandom_ThrowsGridUntouched()
        {
            Grid grid = TestGrid.Parse("R.");
            Assert.Throws<ArgumentNullException>(() => RefillRules.Fill(grid, 3, null));
            TestGrid.AssertRows(new[] { "R." }, grid);
        }

        [Fact]
        public void Fill_NullGridBadCountNullRandom_GridCheckedFirst()
        {
            Assert.Throws<ArgumentNullException>(() => RefillRules.Fill(null, 0, null));
        }

        [Fact]
        public void Fill_BadCountAndNullRandom_ColorCountCheckedBeforeRandom()
        {
            Grid grid = TestGrid.Parse("R.");
            Assert.Throws<ArgumentOutOfRangeException>(() => RefillRules.Fill(grid, 7, null));
            TestGrid.AssertRows(new[] { "R." }, grid);
        }

        [Theory]
        [InlineData(3, 0, 0)]
        [InlineData(0, 0, 3)]
        [InlineData(-1, 0, 0)]
        [InlineData(0, 0, -1)]
        public void Fill_SourceReturnsOutOfRange_ThrowsAndGridUnchanged(int first, int second, int third)
        {
            Grid grid = TestGrid.Parse(".G", "R.", ".B");
            var random = new CountingRandom(first, second, third);

            Assert.Throws<InvalidOperationException>(() => RefillRules.Fill(grid, 3, random));
            TestGrid.AssertRows(new[] { ".G", "R.", ".B" }, grid);
        }
    }
}
