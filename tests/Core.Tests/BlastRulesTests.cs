using System;
using Xunit;
using static BlastGame.Core.Tests.TestGrid;

namespace BlastGame.Core.Tests
{
    public class BlastRulesTests
    {
        private static readonly string[] Example1 = { "RGG", "RRG", "BRG" };

        [Fact]
        public void MinGroupSize_MatchesSpec_Two()
        {
            Assert.Equal(2, BlastRules.MinGroupSize);
        }

        // ---- Spec examples ----

        [Fact]
        public void Blast_Example1RedGroup_ReturnsSpecOrderAndEmptiesOnlyThoseCells()
        {
            Grid grid = Parse(Example1);

            var result = BlastRules.Blast(grid, 1, 1);

            AssertGroup(Pts(1, 0, 0, 1, 1, 1, 0, 2), result);
            AssertRows(new[] { ".GG", "..G", "B.G" }, grid);
        }

        [Theory]
        [InlineData(1, 0)]
        [InlineData(0, 1)]
        [InlineData(1, 1)]
        [InlineData(0, 2)]
        public void Blast_Example1RedGroup_AnyMemberGivesSameResult(int x, int y)
        {
            Grid grid = Parse(Example1);

            AssertGroup(Pts(1, 0, 0, 1, 1, 1, 0, 2), BlastRules.Blast(grid, x, y));
            AssertRows(new[] { ".GG", "..G", "B.G" }, grid);
        }

        [Fact]
        public void Blast_Example1GreenGroup_ReturnsSpecOrderAndKeepsTheRest()
        {
            Grid grid = Parse(Example1);

            AssertGroup(Pts(2, 0, 2, 1, 1, 2, 2, 2), BlastRules.Blast(grid, 2, 0));
            AssertRows(new[] { "R..", "RR.", "BR." }, grid);
        }

        [Fact]
        public void Blast_Example1LoneBlue_ReturnsEmptyAndGridUnchanged()
        {
            Grid grid = Parse(Example1);

            var result = BlastRules.Blast(grid, 0, 0);

            Assert.NotNull(result);
            Assert.Empty(result);
            AssertRows(Example1, grid);
        }

        [Theory]
        [InlineData(0, 0)]
        [InlineData(1, 0)]
        [InlineData(0, 1)]
        [InlineData(1, 1)]
        public void Blast_Example2Diagonal_ReturnsEmptyAndGridUnchanged(int x, int y)
        {
            string[] rows = { "GR", "RG" };
            Grid grid = Parse(rows);

            Assert.Empty(BlastRules.Blast(grid, x, y));
            AssertRows(rows, grid);
        }

        [Fact]
        public void Blast_Example3UniformSixteenBySixteen_Returns256InOrderAndEmptiesGrid()
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

            AssertGroup(expected, BlastRules.Blast(grid, 0, 0));

            for (int y = 0; y < 16; y++)
            {
                for (int x = 0; x < 16; x++)
                {
                    Assert.Equal(Cell.Empty, grid.Get(x, y));
                }
            }
        }

        // ---- Size threshold ----

        [Fact]
        public void Blast_GroupOfExactlyTwoHorizontal_Blasts()
        {
            Grid grid = Parse("RR");

            AssertGroup(Pts(0, 0, 1, 0), BlastRules.Blast(grid, 1, 0));
            AssertRows(new[] { ".." }, grid);
        }

        [Fact]
        public void Blast_GroupOfExactlyTwoVertical_Blasts()
        {
            Grid grid = Parse("R", "R");

            AssertGroup(Pts(0, 0, 0, 1), BlastRules.Blast(grid, 0, 1));
            AssertRows(new[] { ".", "." }, grid);
        }

        [Fact]
        public void Blast_OneByOne_ReturnsEmptyAndKeepsCell()
        {
            Grid grid = Parse("R");

            Assert.Empty(BlastRules.Blast(grid, 0, 0));
            AssertRows(new[] { "R" }, grid);
        }

        [Fact]
        public void Blast_SingleSurroundedByOtherColor_DiagonalSameColorDoesNotJoin()
        {
            string[] rows = { "RBR", "BRB", "RBR" };
            Grid grid = Parse(rows);

            Assert.Empty(BlastRules.Blast(grid, 1, 1));
            AssertRows(rows, grid);
        }

        [Theory]
        [InlineData(TileColor.Red)]
        [InlineData(TileColor.Green)]
        [InlineData(TileColor.Blue)]
        [InlineData(TileColor.Yellow)]
        [InlineData(TileColor.Purple)]
        [InlineData(TileColor.Orange)]
        public void Blast_EachColorPair_BlastsAndLeavesOtherCell(TileColor color)
        {
            TileColor other = (TileColor)(((int)color + 1) % 6);
            Grid grid = Build(3, 1, (x, y) => Cell.OfColor(x < 2 ? color : other));

            AssertGroup(Pts(0, 0, 1, 0), BlastRules.Blast(grid, 0, 0));
            Assert.Equal(Cell.Empty, grid.Get(0, 0));
            Assert.Equal(Cell.Empty, grid.Get(1, 0));
            Assert.Equal(Cell.OfColor(other), grid.Get(2, 0));
        }

        // ---- Corners, edges, whole grid ----

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
        public void Blast_Uniform3x3_AnyStartCellBlastsEverything(int x, int y)
        {
            Grid grid = Filled(3, 3, TileColor.Blue);

            AssertGroup(Pts(0, 0, 1, 0, 2, 0, 0, 1, 1, 1, 2, 1, 0, 2, 1, 2, 2, 2),
                BlastRules.Blast(grid, x, y));
            AssertRows(new[] { "...", "...", "..." }, grid);
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
        public void Blast_Checkerboard3x3_NothingBlastsFromAnyCell(int x, int y)
        {
            string[] rows = { "RBR", "BRB", "RBR" };
            Grid grid = Parse(rows);

            Assert.Empty(BlastRules.Blast(grid, x, y));
            AssertRows(rows, grid);
        }

        [Fact]
        public void Blast_Uniform2x2_BlastsAllFour()
        {
            Grid grid = Filled(2, 2, TileColor.Yellow);

            AssertGroup(Pts(0, 0, 1, 0, 0, 1, 1, 1), BlastRules.Blast(grid, 1, 0));
            AssertRows(new[] { "..", ".." }, grid);
        }

        [Fact]
        public void Blast_UniformSingleColumnAndRow_BlastsWholeStrip()
        {
            Grid column = Filled(1, 4, TileColor.Red);
            Grid row = Filled(4, 1, TileColor.Red);

            AssertGroup(Pts(0, 0, 0, 1, 0, 2, 0, 3), BlastRules.Blast(column, 0, 3));
            AssertGroup(Pts(0, 0, 1, 0, 2, 0, 3, 0), BlastRules.Blast(row, 3, 0));
            AssertRows(new[] { ".", ".", ".", "." }, column);
            AssertRows(new[] { "...." }, row);
        }

        // ---- Cells outside the group never change ----

        [Fact]
        public void Blast_TwoRegionsSameColor_OnlyTouchedRegionIsEmptied()
        {
            Grid grid = Parse("RRGRR", "GGGGG", "RRGRR");

            AssertGroup(Pts(0, 0, 1, 0), BlastRules.Blast(grid, 0, 0));

            AssertRows(new[] { "RRGRR", "GGGGG", "..GRR" }, grid);
        }

        [Fact]
        public void Blast_CrossShape_NeighbourRegionsOfOtherColorStay()
        {
            Grid grid = Parse("RRGRR", "GGGGG", "RRGRR");

            AssertGroup(Pts(2, 0, 0, 1, 1, 1, 2, 1, 3, 1, 4, 1, 2, 2), BlastRules.Blast(grid, 2, 1));

            AssertRows(new[] { "RR.RR", ".....", "RR.RR" }, grid);
        }

        [Fact]
        public void Blast_UShape_InnerOtherColorStays()
        {
            Grid grid = Parse("RBR", "RBR", "RRR");

            AssertGroup(Pts(0, 0, 1, 0, 2, 0, 0, 1, 2, 1, 0, 2, 2, 2), BlastRules.Blast(grid, 0, 0));

            AssertRows(new[] { ".B.", ".B.", "..." }, grid);
        }

        [Fact]
        public void Blast_RingAroundSameColorCenterSeparatedByOtherColor_CenterStays()
        {
            Grid grid = Parse("RRRRR", "RGGGR", "RGRGR", "RGGGR", "RRRRR");

            var result = BlastRules.Blast(grid, 0, 0);

            Assert.Equal(16, result.Count);
            AssertRows(new[] { ".....", ".GGG.", ".GRG.", ".GGG.", "....." }, grid);
        }

        [Fact]
        public void Blast_EmptyCellsAroundGroup_StayEmptyAndNothingFalls()
        {
            Grid grid = Parse("R.R", "RR.", ".RB");

            AssertGroup(Pts(1, 0, 0, 1, 1, 1, 0, 2), BlastRules.Blast(grid, 1, 0));

            AssertRows(new[] { "..R", "...", "..B" }, grid);
        }

        [Fact]
        public void Blast_RectangularWide_UsesXForColumnsAndYForRows()
        {
            Grid grid = Parse("RRRR", "GGGG");

            AssertGroup(Pts(0, 1, 1, 1, 2, 1, 3, 1), BlastRules.Blast(grid, 3, 1));

            AssertRows(new[] { "....", "GGGG" }, grid);
        }

        [Fact]
        public void Blast_RectangularTall_UsesXForColumnsAndYForRows()
        {
            Grid grid = Parse("RG", "RG", "RG", "RG");

            AssertGroup(Pts(1, 0, 1, 1, 1, 2, 1, 3), BlastRules.Blast(grid, 1, 0));

            AssertRows(new[] { "R.", "R.", "R.", "R." }, grid);
        }

        // ---- Empty start ----

        [Fact]
        public void Blast_StartOnEmptyCell_ReturnsEmptyAndGridUnchanged()
        {
            string[] rows = { "RR", ".R" };
            Grid grid = Parse(rows);

            var result = BlastRules.Blast(grid, 0, 0);

            Assert.NotNull(result);
            Assert.Empty(result);
            AssertRows(rows, grid);
        }

        [Fact]
        public void Blast_AllEmptyGrid_ReturnsEmptyAndStaysEmpty()
        {
            var grid = new Grid(3, 2);

            Assert.Empty(BlastRules.Blast(grid, 1, 1));
            AssertRows(new[] { "...", "..." }, grid);
        }

        [Fact]
        public void Blast_EmptyCellBetweenSameColor_BothSingles_NothingBlasts()
        {
            string[] rows = { "R.R" };
            Grid grid = Parse(rows);

            Assert.Empty(BlastRules.Blast(grid, 0, 0));
            Assert.Empty(BlastRules.Blast(grid, 2, 0));
            AssertRows(rows, grid);
        }

        // ---- Repeat and sequence ----

        [Fact]
        public void Blast_SameCellTwice_SecondReturnsEmptyAndGridStaysSame()
        {
            Grid grid = Parse(Example1);

            BlastRules.Blast(grid, 1, 1);
            string[] afterFirst = Dump(grid);

            var second = BlastRules.Blast(grid, 1, 1);

            Assert.Empty(second);
            AssertRows(afterFirst, grid);
        }

        [Fact]
        public void Blast_EveryCellOfBlastedGroup_NowGivesEmptyList()
        {
            Grid grid = Parse(Example1);
            BlastRules.Blast(grid, 1, 1);

            Assert.Empty(BlastRules.Blast(grid, 1, 0));
            Assert.Empty(BlastRules.Blast(grid, 0, 1));
            Assert.Empty(BlastRules.Blast(grid, 0, 2));
            Assert.Empty(GroupFinder.FindGroup(grid, 1, 1));
        }

        [Fact]
        public void Blast_TwoGroupsOneAfterAnother_BothBlastAndLoneCellStays()
        {
            Grid grid = Parse(Example1);

            BlastRules.Blast(grid, 1, 1);
            AssertGroup(Pts(2, 0, 2, 1, 1, 2, 2, 2), BlastRules.Blast(grid, 2, 0));

            AssertRows(new[] { "...", "...", "B.." }, grid);
        }

        [Fact]
        public void Blast_GroupBetweenTwoOthers_RemainingGroupsStayApart()
        {
            Grid grid = Parse("GRG", "GRG");

            AssertGroup(Pts(1, 0, 1, 1), BlastRules.Blast(grid, 1, 0));

            AssertGroup(Pts(0, 0, 0, 1), GroupFinder.FindGroup(grid, 0, 0));
            AssertGroup(Pts(2, 0, 2, 1), GroupFinder.FindGroup(grid, 2, 0));
        }

        // ---- Result list ----

        [Fact]
        public void Blast_Result_MatchesFindGroupBeforeBlast()
        {
            Grid grid = Parse(Example1);
            var expected = GroupFinder.FindGroup(grid, 1, 1);
            int[] expectedXy = new int[expected.Count * 2];
            for (int i = 0; i < expected.Count; i++)
            {
                expectedXy[2 * i] = expected[i].X;
                expectedXy[2 * i + 1] = expected[i].Y;
            }

            AssertGroup(expectedXy, BlastRules.Blast(grid, 1, 1));
        }

        [Fact]
        public void Blast_EveryReturnedCellHadTheTouchedColorBeforeAndIsEmptyAfter()
        {
            Grid grid = Parse(Example1);
            Cell touched = grid.Get(1, 1);
            Assert.Equal(Cell.OfColor(TileColor.Red), touched);
            Grid before = Parse(Example1);

            var result = BlastRules.Blast(grid, 1, 1);

            Assert.Equal(4, result.Count);
            foreach (CellPos pos in result)
            {
                Assert.Equal(touched, before.Get(pos.X, pos.Y));
                Assert.Equal(Cell.Empty, grid.Get(pos.X, pos.Y));
            }
        }

        [Fact]
        public void Blast_Result_IsNotAffectedByLaterBlastsOrSets()
        {
            Grid grid = Parse("RRGG");

            var first = BlastRules.Blast(grid, 0, 0);
            BlastRules.Blast(grid, 2, 0);
            grid.Set(0, 0, Cell.OfColor(TileColor.Blue));

            AssertGroup(Pts(0, 0, 1, 0), first);
        }

        // ---- Errors ----

        [Fact]
        public void Blast_NullGrid_ThrowsArgumentNull()
        {
            Assert.Throws<ArgumentNullException>(() => BlastRules.Blast(null, 0, 0));
        }

        [Fact]
        public void Blast_NullGridAndBadCoordinates_ThrowsArgumentNullFirst()
        {
            Assert.Throws<ArgumentNullException>(() => BlastRules.Blast(null, -1, -1));
            Assert.Throws<ArgumentNullException>(() => BlastRules.Blast(null, 99, 99));
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
        public void Blast_OutsideCoordinates_ThrowsArgumentOutOfRangeAndLeavesGridUnchanged(int x, int y)
        {
            string[] rows = { "RRR", "RRR" }; // 3 wide, 2 tall, one big group
            Grid grid = Parse(rows);

            Assert.Throws<ArgumentOutOfRangeException>(() => BlastRules.Blast(grid, x, y));

            AssertRows(rows, grid);
        }

        [Fact]
        public void Blast_OneByOne_NeighbourCoordinatesThrow()
        {
            Grid grid = Parse("R");

            Assert.Throws<ArgumentOutOfRangeException>(() => BlastRules.Blast(grid, 1, 0));
            Assert.Throws<ArgumentOutOfRangeException>(() => BlastRules.Blast(grid, 0, 1));
            Assert.Throws<ArgumentOutOfRangeException>(() => BlastRules.Blast(grid, -1, 0));
            Assert.Throws<ArgumentOutOfRangeException>(() => BlastRules.Blast(grid, 0, -1));
            AssertRows(new[] { "R" }, grid);
        }

        [Fact]
        public void Blast_SixteenBySixteen_JustOutsideCornersThrowAndGridStaysFull()
        {
            Grid grid = Filled(16, 16, TileColor.Red);

            Assert.Throws<ArgumentOutOfRangeException>(() => BlastRules.Blast(grid, 16, 0));
            Assert.Throws<ArgumentOutOfRangeException>(() => BlastRules.Blast(grid, 0, 16));
            Assert.Throws<ArgumentOutOfRangeException>(() => BlastRules.Blast(grid, 16, 15));
            Assert.Throws<ArgumentOutOfRangeException>(() => BlastRules.Blast(grid, 15, 16));

            for (int y = 0; y < 16; y++)
            {
                for (int x = 0; x < 16; x++)
                {
                    Assert.Equal(Cell.OfColor(TileColor.Red), grid.Get(x, y));
                }
            }
        }
    }
}
