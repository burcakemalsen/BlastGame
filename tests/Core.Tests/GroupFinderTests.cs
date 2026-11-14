using System;
using Xunit;
using static BlastGame.Core.Tests.TestGrid;

namespace BlastGame.Core.Tests
{
    public class GroupFinderTests
    {
        // ---- Spec examples ----

        private static readonly string[] Example1 = { "RGG", "RRG", "BRG" };

        [Theory]
        [InlineData(1, 0)]
        [InlineData(0, 1)]
        [InlineData(1, 1)]
        [InlineData(0, 2)]
        public void FindGroup_Example1RedGroup_FromAnyMemberReturnsSpecOrder(int x, int y)
        {
            Grid grid = Parse(Example1);

            AssertGroup(Pts(1, 0, 0, 1, 1, 1, 0, 2), GroupFinder.FindGroup(grid, x, y));
        }

        [Theory]
        [InlineData(2, 0)]
        [InlineData(2, 1)]
        [InlineData(1, 2)]
        [InlineData(2, 2)]
        public void FindGroup_Example1GreenGroup_FromAnyMemberReturnsSpecOrder(int x, int y)
        {
            Grid grid = Parse(Example1);

            AssertGroup(Pts(2, 0, 2, 1, 1, 2, 2, 2), GroupFinder.FindGroup(grid, x, y));
        }

        [Fact]
        public void FindGroup_Example1LoneBlue_ReturnsOnlyThatCell()
        {
            Grid grid = Parse(Example1);

            AssertGroup(Pts(0, 0), GroupFinder.FindGroup(grid, 0, 0));
        }

        [Theory]
        [InlineData(0, 0)]
        [InlineData(1, 0)]
        [InlineData(0, 1)]
        [InlineData(1, 1)]
        public void FindGroup_Example2Diagonal_DoesNotConnect(int x, int y)
        {
            Grid grid = Parse("GR", "RG");

            AssertGroup(Pts(x, y), GroupFinder.FindGroup(grid, x, y));
        }

        [Fact]
        public void FindGroup_Example3UniformSixteenBySixteen_ReturnsAll256InRowOrder()
        {
            Grid grid = Filled(16, 16, TileColor.Red);
            var expected = new int[512];
            int i = 0;
            for (int y = 0; y < 16; y++)
            {
                for (int x = 0; x < 16; x++)
                {
                    expected[i++] = x;
                    expected[i++] = y;
                }
            }

            AssertGroup(expected, GroupFinder.FindGroup(grid, 7, 7));
        }

        // ---- Corners and edges ----

        [Theory]
        [InlineData(0, 0)]
        [InlineData(2, 0)]
        [InlineData(0, 2)]
        [InlineData(2, 2)]
        [InlineData(1, 0)]
        [InlineData(0, 1)]
        [InlineData(2, 1)]
        [InlineData(1, 2)]
        [InlineData(1, 1)]
        public void FindGroup_Uniform3x3_AnyStartCellReturnsWholeGrid(int x, int y)
        {
            Grid grid = Filled(3, 3, TileColor.Blue);

            AssertGroup(Pts(0, 0, 1, 0, 2, 0, 0, 1, 1, 1, 2, 1, 0, 2, 1, 2, 2, 2),
                GroupFinder.FindGroup(grid, x, y));
        }

        [Theory]
        [InlineData(0, 0)]
        [InlineData(3, 0)]
        [InlineData(0, 1)]
        [InlineData(3, 1)]
        [InlineData(1, 0)]
        [InlineData(2, 1)]
        public void FindGroup_UniformWide4x2_BorderStartsReturnWholeGrid(int x, int y)
        {
            Grid grid = Filled(4, 2, TileColor.Green);

            AssertGroup(Pts(0, 0, 1, 0, 2, 0, 3, 0, 0, 1, 1, 1, 2, 1, 3, 1),
                GroupFinder.FindGroup(grid, x, y));
        }

        [Theory]
        [InlineData(0, 0)]
        [InlineData(1, 0)]
        [InlineData(0, 3)]
        [InlineData(1, 3)]
        [InlineData(0, 1)]
        [InlineData(1, 2)]
        public void FindGroup_UniformTall2x4_BorderStartsReturnWholeGrid(int x, int y)
        {
            Grid grid = Filled(2, 4, TileColor.Green);

            AssertGroup(Pts(0, 0, 1, 0, 0, 1, 1, 1, 0, 2, 1, 2, 0, 3, 1, 3),
                GroupFinder.FindGroup(grid, x, y));
        }

        [Theory]
        [InlineData(0, 0)]
        [InlineData(1, 0)]
        [InlineData(2, 0)]
        [InlineData(0, 1)]
        [InlineData(1, 1)]
        [InlineData(2, 1)]
        [InlineData(0, 2)]
        [InlineData(1, 2)]
        [InlineData(2, 2)]
        public void FindGroup_Checkerboard3x3_EveryCellIsAloneIncludingBorders(int x, int y)
        {
            Grid grid = Parse("RBR", "BRB", "RBR");

            AssertGroup(Pts(x, y), GroupFinder.FindGroup(grid, x, y));
        }

        [Fact]
        public void FindGroup_RowEndNextToNextRowStart_DoesNotWrapAround()
        {
            // (2,0) and (0,1) are neighbours in a flat array but not in the grid.
            Grid grid = Parse("RGG", "GGR");

            AssertGroup(Pts(2, 0), GroupFinder.FindGroup(grid, 2, 0));
            AssertGroup(Pts(0, 1), GroupFinder.FindGroup(grid, 0, 1));
        }

        [Fact]
        public void FindGroup_RowEndNextToNextRowStart_OtherGroupStillConnects()
        {
            Grid grid = Parse("RGG", "GGR");

            AssertGroup(Pts(0, 0, 1, 0, 1, 1, 2, 1), GroupFinder.FindGroup(grid, 1, 1));
        }

        // ---- Strips ----

        [Fact]
        public void FindGroup_OneByOne_ReturnsThatCell()
        {
            Grid grid = Parse("R");

            AssertGroup(Pts(0, 0), GroupFinder.FindGroup(grid, 0, 0));
        }

        [Fact]
        public void FindGroup_UniformSingleColumn_ReturnsWholeColumnBottomUp()
        {
            Grid grid = Filled(1, 5, TileColor.Red);

            AssertGroup(Pts(0, 0, 0, 1, 0, 2, 0, 3, 0, 4), GroupFinder.FindGroup(grid, 0, 2));
        }

        [Fact]
        public void FindGroup_UniformSingleRow_ReturnsWholeRowLeftToRight()
        {
            Grid grid = Filled(5, 1, TileColor.Red);

            AssertGroup(Pts(0, 0, 1, 0, 2, 0, 3, 0, 4, 0), GroupFinder.FindGroup(grid, 4, 0));
        }

        [Fact]
        public void FindGroup_ColumnBrokenByOtherColor_ReturnsOnlyOwnPart()
        {
            Grid grid = Parse("R", "R", "G", "R", "R"); // y=4..0

            AssertGroup(Pts(0, 0, 0, 1), GroupFinder.FindGroup(grid, 0, 0));
            AssertGroup(Pts(0, 3, 0, 4), GroupFinder.FindGroup(grid, 0, 4));
            AssertGroup(Pts(0, 2), GroupFinder.FindGroup(grid, 0, 2));
        }

        [Fact]
        public void FindGroup_RowBrokenByOtherColor_ReturnsOnlyOwnPart()
        {
            Grid grid = Parse("RRGRR");

            AssertGroup(Pts(0, 0, 1, 0), GroupFinder.FindGroup(grid, 1, 0));
            AssertGroup(Pts(3, 0, 4, 0), GroupFinder.FindGroup(grid, 4, 0));
        }

        [Fact]
        public void FindGroup_UniformSixteenWideStrip_ReturnsSixteenInOrder()
        {
            Grid grid = Filled(16, 1, TileColor.Orange);
            var expected = new int[32];
            for (int x = 0; x < 16; x++)
            {
                expected[2 * x] = x;
                expected[2 * x + 1] = 0;
            }

            AssertGroup(expected, GroupFinder.FindGroup(grid, 15, 0));
        }

        [Fact]
        public void FindGroup_UniformSixteenTallStrip_ReturnsSixteenInOrder()
        {
            Grid grid = Filled(1, 16, TileColor.Orange);
            var expected = new int[32];
            for (int y = 0; y < 16; y++)
            {
                expected[2 * y] = 0;
                expected[2 * y + 1] = y;
            }

            AssertGroup(expected, GroupFinder.FindGroup(grid, 0, 15));
        }

        // ---- Whole grid, small ----

        [Fact]
        public void FindGroup_Uniform2x2_ReturnsFourInRowOrder()
        {
            Grid grid = Filled(2, 2, TileColor.Yellow);

            AssertGroup(Pts(0, 0, 1, 0, 0, 1, 1, 1), GroupFinder.FindGroup(grid, 1, 1));
        }

        [Theory]
        [InlineData(TileColor.Red)]
        [InlineData(TileColor.Green)]
        [InlineData(TileColor.Blue)]
        [InlineData(TileColor.Yellow)]
        [InlineData(TileColor.Purple)]
        [InlineData(TileColor.Orange)]
        public void FindGroup_EachColor_GroupsOnlyThatColor(TileColor color)
        {
            Grid grid = Build(3, 1, (x, y) =>
                x < 2 ? Cell.OfColor(color) : Cell.OfColor((TileColor)(((int)color + 1) % 6)));

            AssertGroup(Pts(0, 0, 1, 0), GroupFinder.FindGroup(grid, 0, 0));
            AssertGroup(Pts(2, 0), GroupFinder.FindGroup(grid, 2, 0));
        }

        // ---- Separate regions ----

        [Fact]
        public void FindGroup_SameColorTwoRegions_OnlyTouchedRegionReturned()
        {
            Grid grid = Parse("RRGRR", "GGGGG", "RRGRR");

            AssertGroup(Pts(0, 0, 1, 0), GroupFinder.FindGroup(grid, 0, 0));
            AssertGroup(Pts(3, 0, 4, 0), GroupFinder.FindGroup(grid, 4, 0));
            AssertGroup(Pts(0, 2, 1, 2), GroupFinder.FindGroup(grid, 1, 2));
            AssertGroup(Pts(3, 2, 4, 2), GroupFinder.FindGroup(grid, 3, 2));
        }

        [Fact]
        public void FindGroup_CrossShape_ReturnsRowAndBothArms()
        {
            Grid grid = Parse("RRGRR", "GGGGG", "RRGRR");

            AssertGroup(Pts(2, 0, 0, 1, 1, 1, 2, 1, 3, 1, 4, 1, 2, 2), GroupFinder.FindGroup(grid, 2, 1));
        }

        [Fact]
        public void FindGroup_SingleSurroundedByOtherColor_ReturnsOnlyItselfEvenWithDiagonalSameColor()
        {
            Grid grid = Parse("RBR", "BRB", "RBR");

            AssertGroup(Pts(1, 1), GroupFinder.FindGroup(grid, 1, 1));
        }

        // ---- Winding shapes ----

        [Fact]
        public void FindGroup_UShape_ReturnsAllSevenInRowOrder()
        {
            Grid grid = Parse("RBR", "RBR", "RRR");

            AssertGroup(Pts(0, 0, 1, 0, 2, 0, 0, 1, 2, 1, 0, 2, 2, 2), GroupFinder.FindGroup(grid, 2, 2));
            AssertGroup(Pts(1, 1, 1, 2), GroupFinder.FindGroup(grid, 1, 2));
        }

        [Fact]
        public void FindGroup_Snake5x5_FollowsWholePathFromEitherEnd()
        {
            Grid grid = Parse("RRRRR", "GGGGR", "RRRGR", "RGRGR", "RGRRR");
            int[] expected = Pts(
                0, 0, 2, 0, 3, 0, 4, 0,
                0, 1, 2, 1, 4, 1,
                0, 2, 1, 2, 2, 2, 4, 2,
                4, 3,
                0, 4, 1, 4, 2, 4, 3, 4, 4, 4);

            AssertGroup(expected, GroupFinder.FindGroup(grid, 0, 0));
            AssertGroup(expected, GroupFinder.FindGroup(grid, 0, 4));
        }

        [Fact]
        public void FindGroup_Snake5x5_GreenPartsAreSeparate()
        {
            Grid grid = Parse("RRRRR", "GGGGR", "RRRGR", "RGRGR", "RGRRR");

            AssertGroup(Pts(1, 0, 1, 1), GroupFinder.FindGroup(grid, 1, 0));
            AssertGroup(Pts(3, 1, 3, 2, 0, 3, 1, 3, 2, 3, 3, 3), GroupFinder.FindGroup(grid, 0, 3));
        }

        [Fact]
        public void FindGroup_RingAroundOtherColor_ReturnsEightWithoutCenter()
        {
            Grid grid = Parse("RRR", "RGR", "RRR");

            AssertGroup(Pts(0, 0, 1, 0, 2, 0, 0, 1, 2, 1, 0, 2, 1, 2, 2, 2), GroupFinder.FindGroup(grid, 0, 0));
            AssertGroup(Pts(1, 1), GroupFinder.FindGroup(grid, 1, 1));
        }

        [Fact]
        public void FindGroup_OuterRingAndInnerSameColor_AreSeparateGroups()
        {
            Grid grid = Parse("RRRRR", "RGGGR", "RGRGR", "RGGGR", "RRRRR");
            var outer = new System.Collections.Generic.List<int>();
            for (int y = 0; y < 5; y++)
            {
                for (int x = 0; x < 5; x++)
                {
                    if (x == 0 || x == 4 || y == 0 || y == 4)
                    {
                        outer.Add(x);
                        outer.Add(y);
                    }
                }
            }

            AssertGroup(Pts(2, 2), GroupFinder.FindGroup(grid, 2, 2));
            AssertGroup(outer.ToArray(), GroupFinder.FindGroup(grid, 0, 0));
            Assert.Equal(16, outer.Count / 2);
        }

        [Fact]
        public void FindGroup_SerpentineSixteenBySixteen_ReturnsLongPathInRowOrder()
        {
            Grid grid = Build(16, 16, (x, y) =>
            {
                bool red = y % 2 == 0 || x == (y % 4 == 1 ? 15 : 0);
                return Cell.OfColor(red ? TileColor.Red : TileColor.Green);
            });
            var expected = new System.Collections.Generic.List<int>();
            for (int y = 0; y < 16; y++)
            {
                for (int x = 0; x < 16; x++)
                {
                    if (y % 2 == 0 || x == (y % 4 == 1 ? 15 : 0))
                    {
                        expected.Add(x);
                        expected.Add(y);
                    }
                }
            }

            AssertGroup(expected.ToArray(), GroupFinder.FindGroup(grid, 0, 0));
            Assert.Equal(136, expected.Count / 2);
        }

        [Fact]
        public void FindGroup_ColumnStripesSixteenBySixteen_ReturnsOneColumn()
        {
            Grid grid = Build(16, 16, (x, y) => Cell.OfColor(x % 2 == 0 ? TileColor.Red : TileColor.Green));
            var expected = new int[32];
            for (int y = 0; y < 16; y++)
            {
                expected[2 * y] = 5;
                expected[2 * y + 1] = y;
            }

            AssertGroup(expected, GroupFinder.FindGroup(grid, 5, 8));
        }

        // ---- Empty cells ----

        [Fact]
        public void FindGroup_StartOnEmptyCell_ReturnsEmptyList()
        {
            Grid grid = Parse("RR", ".R");

            var result = GroupFinder.FindGroup(grid, 0, 0);

            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public void FindGroup_EmptyOneByOne_ReturnsEmptyList()
        {
            var result = GroupFinder.FindGroup(new Grid(1, 1), 0, 0);

            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public void FindGroup_AllEmptyGrid_AnyCellReturnsEmptyList()
        {
            var grid = new Grid(4, 3);

            for (int y = 0; y < 3; y++)
            {
                for (int x = 0; x < 4; x++)
                {
                    Assert.Empty(GroupFinder.FindGroup(grid, x, y));
                }
            }
        }

        [Fact]
        public void FindGroup_EmptyCellBetweenSameColor_SplitsGroups()
        {
            Grid grid = Parse("R.R");

            AssertGroup(Pts(0, 0), GroupFinder.FindGroup(grid, 0, 0));
            AssertGroup(Pts(2, 0), GroupFinder.FindGroup(grid, 2, 0));
        }

        [Fact]
        public void FindGroup_EmptyColumnSplitsVertically_SplitsGroups()
        {
            Grid grid = Parse("R", ".", "R");

            AssertGroup(Pts(0, 0), GroupFinder.FindGroup(grid, 0, 0));
            AssertGroup(Pts(0, 2), GroupFinder.FindGroup(grid, 0, 2));
        }

        [Fact]
        public void FindGroup_EmptyCellsInsideRegion_GroupIgnoresThemButKeepsConnectedPart()
        {
            Grid grid = Parse("RR.RR", "R...R", "RRRRR");

            AssertGroup(Pts(0, 0, 1, 0, 2, 0, 3, 0, 4, 0, 0, 1, 4, 1, 0, 2, 1, 2, 3, 2, 4, 2),
                GroupFinder.FindGroup(grid, 0, 2));
        }

        // ---- Rectangular grids: x and y must not be mixed up ----

        [Fact]
        public void FindGroup_Wide4x2_HorizontalBandsUseXForColumnsAndYForRows()
        {
            Grid grid = Parse("RRRR", "GGGG");

            AssertGroup(Pts(0, 1, 1, 1, 2, 1, 3, 1), GroupFinder.FindGroup(grid, 3, 1));
            AssertGroup(Pts(0, 0, 1, 0, 2, 0, 3, 0), GroupFinder.FindGroup(grid, 0, 0));
        }

        [Fact]
        public void FindGroup_Tall2x4_VerticalBandsUseXForColumnsAndYForRows()
        {
            Grid grid = Parse("RG", "RG", "RG", "RG");

            AssertGroup(Pts(0, 0, 0, 1, 0, 2, 0, 3), GroupFinder.FindGroup(grid, 0, 3));
            AssertGroup(Pts(1, 0, 1, 1, 1, 2, 1, 3), GroupFinder.FindGroup(grid, 1, 0));
        }

        [Fact]
        public void FindGroup_RectangularGridWithDistinctCells_OnlyReturnsCellsInsideTheGrid()
        {
            Grid grid = Parse("RGB", "RYO");

            AssertGroup(Pts(0, 0, 0, 1), GroupFinder.FindGroup(grid, 0, 1));
            AssertGroup(Pts(2, 1), GroupFinder.FindGroup(grid, 2, 1));
            AssertGroup(Pts(2, 0), GroupFinder.FindGroup(grid, 2, 0));
        }

        // ---- Grid is not modified, results are not shared ----

        [Fact]
        public void FindGroup_Call_LeavesEveryCellUnchanged()
        {
            string[] rows = { "RGG", "RRG", "BRG" };
            Grid grid = Parse(rows);

            GroupFinder.FindGroup(grid, 1, 1);
            GroupFinder.FindGroup(grid, 2, 0);
            GroupFinder.FindGroup(grid, 0, 0);

            AssertRows(rows, grid);
        }

        [Fact]
        public void FindGroup_OnEmptyCell_LeavesGridUnchanged()
        {
            string[] rows = { "R.", "RR" };
            Grid grid = Parse(rows);

            GroupFinder.FindGroup(grid, 1, 1);

            AssertRows(rows, grid);
        }

        [Fact]
        public void FindGroup_SameCallTwice_ReturnsSameContent()
        {
            Grid grid = Parse(Example1);

            var first = GroupFinder.FindGroup(grid, 1, 1);
            var second = GroupFinder.FindGroup(grid, 1, 1);

            AssertGroup(Pts(1, 0, 0, 1, 1, 1, 0, 2), first);
            AssertGroup(Pts(1, 0, 0, 1, 1, 1, 0, 2), second);
        }

        [Fact]
        public void FindGroup_AfterGridChanges_ReflectsNewContent()
        {
            Grid grid = Parse("RR");

            AssertGroup(Pts(0, 0, 1, 0), GroupFinder.FindGroup(grid, 0, 0));

            grid.Set(1, 0, Cell.OfColor(TileColor.Green));

            AssertGroup(Pts(0, 0), GroupFinder.FindGroup(grid, 0, 0));
        }

        [Fact]
        public void FindGroup_EarlierResult_IsNotAffectedByLaterChangesToTheGrid()
        {
            Grid grid = Parse("RR");

            var result = GroupFinder.FindGroup(grid, 0, 0);
            grid.Set(0, 0, Cell.Empty);
            grid.Set(1, 0, Cell.Empty);

            AssertGroup(Pts(0, 0, 1, 0), result);
        }

        // ---- Errors ----

        [Fact]
        public void FindGroup_NullGrid_ThrowsArgumentNull()
        {
            Assert.Throws<ArgumentNullException>(() => GroupFinder.FindGroup(null, 0, 0));
        }

        [Fact]
        public void FindGroup_NullGridAndBadCoordinates_ThrowsArgumentNullFirst()
        {
            Assert.Throws<ArgumentNullException>(() => GroupFinder.FindGroup(null, -1, -1));
            Assert.Throws<ArgumentNullException>(() => GroupFinder.FindGroup(null, 99, 99));
        }

        [Theory]
        [InlineData(-1, 0)]
        [InlineData(0, -1)]
        [InlineData(-1, -1)]
        [InlineData(3, 0)]
        [InlineData(0, 2)]
        [InlineData(3, 2)]
        [InlineData(3, 1)]
        [InlineData(2, 2)]
        [InlineData(1, 2)] // swapped (x, y) of the inside cell (2, 1)
        [InlineData(int.MinValue, 0)]
        [InlineData(0, int.MinValue)]
        [InlineData(int.MaxValue, 0)]
        [InlineData(0, int.MaxValue)]
        public void FindGroup_OutsideCoordinates_ThrowsArgumentOutOfRange(int x, int y)
        {
            Grid grid = Parse("RRR", "RRR"); // 3 wide, 2 tall

            Assert.Throws<ArgumentOutOfRangeException>(() => GroupFinder.FindGroup(grid, x, y));
        }

        [Fact]
        public void FindGroup_OutsideCoordinates_LeavesGridUnchanged()
        {
            string[] rows = { "RRR", "RRR" };
            Grid grid = Parse(rows);

            Assert.Throws<ArgumentOutOfRangeException>(() => GroupFinder.FindGroup(grid, 3, 0));
            Assert.Throws<ArgumentOutOfRangeException>(() => GroupFinder.FindGroup(grid, 0, 2));
            Assert.Throws<ArgumentOutOfRangeException>(() => GroupFinder.FindGroup(grid, -1, 0));

            AssertRows(rows, grid);
        }

        [Fact]
        public void FindGroup_OneByOne_NeighbourCoordinatesThrow()
        {
            Grid grid = Parse("R");

            Assert.Throws<ArgumentOutOfRangeException>(() => GroupFinder.FindGroup(grid, 1, 0));
            Assert.Throws<ArgumentOutOfRangeException>(() => GroupFinder.FindGroup(grid, 0, 1));
            Assert.Throws<ArgumentOutOfRangeException>(() => GroupFinder.FindGroup(grid, -1, 0));
            Assert.Throws<ArgumentOutOfRangeException>(() => GroupFinder.FindGroup(grid, 0, -1));
        }

        [Fact]
        public void FindGroup_SixteenBySixteen_JustOutsideCornersThrow()
        {
            Grid grid = Filled(16, 16, TileColor.Red);

            Assert.Throws<ArgumentOutOfRangeException>(() => GroupFinder.FindGroup(grid, 16, 0));
            Assert.Throws<ArgumentOutOfRangeException>(() => GroupFinder.FindGroup(grid, 0, 16));
            Assert.Throws<ArgumentOutOfRangeException>(() => GroupFinder.FindGroup(grid, 16, 15));
            Assert.Throws<ArgumentOutOfRangeException>(() => GroupFinder.FindGroup(grid, 15, 16));
        }
    }
}
