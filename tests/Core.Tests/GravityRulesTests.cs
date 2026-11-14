using System;
using System.Collections.Generic;
using Xunit;
using static BlastGame.Core.Tests.TurnTestHelpers;

namespace BlastGame.Core.Tests
{
    public class GravityRulesTests
    {
        [Fact]
        public void Collapse_NullGrid_ThrowsArgumentNull()
        {
            Assert.Throws<ArgumentNullException>(() => GravityRules.Collapse(null));
        }

        [Fact]
        public void Collapse_Example1_ExactMovesAndGrid()
        {
            Grid grid = TestGrid.Parse("G.B", ".R.", "R.B", "..G");

            IReadOnlyList<CellMove> moves = GravityRules.Collapse(grid);

            AssertMoves(new[] { 0, 1, 0, 0, 0, 3, 0, 1, 1, 2, 1, 0, 2, 3, 2, 2 }, moves);
            TestGrid.AssertRows(new[] { "...", "..B", "G.B", "RRG" }, grid);
        }

        [Fact]
        public void Collapse_NoEmptyCells_EmptyListGridUnchanged()
        {
            Grid grid = TestGrid.Parse("RGB", "BYP", "OGR");

            Assert.Empty(GravityRules.Collapse(grid));
            TestGrid.AssertRows(new[] { "RGB", "BYP", "OGR" }, grid);
        }

        [Fact]
        public void Collapse_GapsAlreadyAtTop_EmptyListGridUnchanged()
        {
            Grid grid = TestGrid.Parse("...", "R..", "GB.", "YPO");

            Assert.Empty(GravityRules.Collapse(grid));
            TestGrid.AssertRows(new[] { "...", "R..", "GB.", "YPO" }, grid);
        }

        [Fact]
        public void Collapse_ColumnFullyEmpty_StaysEmptyNoMoves()
        {
            Grid grid = TestGrid.Parse(".", ".", ".", ".");

            Assert.Empty(GravityRules.Collapse(grid));
            TestGrid.AssertRows(new[] { ".", ".", ".", "." }, grid);
        }

        [Fact]
        public void Collapse_SingleGapAtBottom_WholeColumnShiftsDownOne()
        {
            Grid grid = TestGrid.Parse("B", "G", "R", ".");

            AssertMoves(new[] { 0, 1, 0, 0, 0, 2, 0, 1, 0, 3, 0, 2 }, GravityRules.Collapse(grid));
            TestGrid.AssertRows(new[] { ".", "B", "G", "R" }, grid);
        }

        [Fact]
        public void Collapse_GapInMiddle_OnlyCellsAboveMove()
        {
            Grid grid = TestGrid.Parse("Y", "B", ".", "G", "R");

            AssertMoves(new[] { 0, 3, 0, 2, 0, 4, 0, 3 }, GravityRules.Collapse(grid));
            TestGrid.AssertRows(new[] { ".", "Y", "B", "G", "R" }, grid);
        }

        [Fact]
        public void Collapse_SeveralGapPieces_OrderOfColorsPreserved()
        {
            // top to bottom: y6=R, y5=., y4=G, y3=., y2=., y1=B, y0=.
            Grid grid = TestGrid.Parse("R", ".", "G", ".", ".", "B", ".");

            AssertMoves(new[] { 0, 1, 0, 0, 0, 4, 0, 1, 0, 6, 0, 2 }, GravityRules.Collapse(grid));
            TestGrid.AssertRows(new[] { ".", ".", ".", ".", "R", "G", "B" }, grid);
        }

        [Fact]
        public void Collapse_AllSixColorsWithGaps_KindAndColorCarried()
        {
            Grid grid = TestGrid.Parse("O", ".", "P", "Y", ".", "B", "G", ".", "R", ".");

            GravityRules.Collapse(grid);

            TestGrid.AssertRows(new[] { ".", ".", ".", ".", "O", "P", "Y", "B", "G", "R" }, grid);
            for (int y = 0; y < 6; y++)
            {
                Assert.Equal(CellKind.Color, grid.Get(0, y).Kind);
            }
        }

        [Fact]
        public void Collapse_SingleCellAtTopOf16TallColumn_FallsToBottom()
        {
            var grid = new Grid(1, Grid.MaxSize);
            grid.Set(0, 15, Cell.OfColor(TileColor.Purple));

            AssertMoves(new[] { 0, 15, 0, 0 }, GravityRules.Collapse(grid));
            Assert.Equal(Cell.OfColor(TileColor.Purple), grid.Get(0, 0));
            Assert.True(grid.Get(0, 15).IsEmpty);
        }

        [Fact]
        public void Collapse_1x1Empty_NoMoves()
        {
            Grid grid = TestGrid.Parse(".");
            Assert.Empty(GravityRules.Collapse(grid));
            TestGrid.AssertRows(new[] { "." }, grid);
        }

        [Fact]
        public void Collapse_1x1Filled_NoMoves()
        {
            Grid grid = TestGrid.Parse("G");
            Assert.Empty(GravityRules.Collapse(grid));
            TestGrid.AssertRows(new[] { "G" }, grid);
        }

        [Fact]
        public void Collapse_SingleRowWithGaps_NothingMovesSideways()
        {
            Grid grid = TestGrid.Parse("R.G..B");

            Assert.Empty(GravityRules.Collapse(grid));
            TestGrid.AssertRows(new[] { "R.G..B" }, grid);
        }

        [Fact]
        public void Collapse_ColumnsIndependent_FullAndEmptyNeighboursUntouched()
        {
            Grid grid = TestGrid.Parse("R.G", ".YB", "B.P", ".PO");

            AssertMoves(new[] { 0, 1, 0, 0, 0, 3, 0, 1, 1, 2, 1, 1 }, GravityRules.Collapse(grid));
            TestGrid.AssertRows(new[] { "..G", "..B", "RYP", "BPO" }, grid);
        }

        [Fact]
        public void Collapse_16x16BottomRowEmpty_EveryCellShiftsDownInOrder()
        {
            Grid grid = TestGrid.Build(16, 16,
                (x, y) => y == 0 ? Cell.Empty : Cell.OfColor((TileColor)((x + y) % 6)));
            Grid before = Clone(grid);

            IReadOnlyList<CellMove> moves = GravityRules.Collapse(grid);

            var expected = new List<int>();
            for (int x = 0; x < 16; x++)
            {
                for (int y = 1; y < 16; y++)
                {
                    expected.AddRange(new[] { x, y, x, y - 1 });
                }
            }

            AssertMoves(expected.ToArray(), moves);
            for (int x = 0; x < 16; x++)
            {
                for (int y = 0; y < 15; y++)
                {
                    Assert.Equal(before.Get(x, y + 1), grid.Get(x, y));
                }

                Assert.True(grid.Get(x, 15).IsEmpty);
            }
        }

        [Fact]
        public void Collapse_CalledTwice_SecondCallEmptyGridSame()
        {
            Grid grid = TestGrid.Parse("G.B", ".R.", "R.B", "..G");
            GravityRules.Collapse(grid);

            Assert.Empty(GravityRules.Collapse(grid));
            TestGrid.AssertRows(new[] { "...", "..B", "G.B", "RRG" }, grid);
        }
    }
}
