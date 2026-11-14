using System;
using System.Collections.Generic;
using Xunit;
using static BlastGame.Core.Tests.TurnTestHelpers;

namespace BlastGame.Core.Tests
{
    public class TurnRulesTests
    {
        private static readonly string[] Example3 = { "BGG", "RRG", "BRB" };

        private static void AssertNothingHappened(TurnResult result)
        {
            Assert.NotNull(result);
            Assert.Empty(result.Blasted);
            Assert.Empty(result.Moves);
            Assert.Empty(result.Filled);
            Assert.False(result.HasBlasted);
        }

        private static void AssertNoEmptyCell(Grid grid)
        {
            for (int y = 0; y < grid.Height; y++)
            {
                for (int x = 0; x < grid.Width; x++)
                {
                    Assert.False(grid.Get(x, y).IsEmpty, "Empty cell at (" + x + "," + y + ")");
                }
            }
        }

        [Fact]
        public void Tap_Example3Group_ExactListsAndGrid()
        {
            Grid grid = TestGrid.Parse(Example3);
            var random = CountingRandom.Constant(2);

            TurnResult result = TurnRules.Tap(grid, 1, 0, 3, random);

            TestGrid.AssertGroup(TestGrid.Pts(1, 0, 0, 1, 1, 1), result.Blasted);
            AssertMoves(new[] { 0, 2, 0, 1, 1, 2, 1, 0 }, result.Moves);
            TestGrid.AssertGroup(TestGrid.Pts(1, 1, 0, 2, 1, 2), result.Filled);
            Assert.True(result.HasBlasted);
            TestGrid.AssertRows(new[] { "BBG", "BBG", "BGB" }, grid);
            Assert.Equal(3, random.Calls);
            Assert.Equal(new[] { 3, 3, 3 }, random.MaxArgs);
        }

        [Fact]
        public void Tap_Example3SingleCell_NothingChangesSourceNotCalled()
        {
            Grid grid = TestGrid.Parse(Example3);
            var random = CountingRandom.Constant(2);

            AssertNothingHappened(TurnRules.Tap(grid, 0, 0, 3, random));
            TestGrid.AssertRows(Example3, grid);
            Assert.Equal(0, random.Calls);
        }

        [Fact]
        public void Tap_EmptyCell_NothingChangesSourceNotCalled()
        {
            Grid grid = TestGrid.Parse("R.", "RG");
            var random = CountingRandom.Constant(0);

            AssertNothingHappened(TurnRules.Tap(grid, 1, 1, 3, random));
            TestGrid.AssertRows(new[] { "R.", "RG" }, grid);
            Assert.Equal(0, random.Calls);
        }

        [Fact]
        public void Tap_1x1Grid_NoBlastSourceNotCalled()
        {
            Grid grid = TestGrid.Parse("Y");
            var random = CountingRandom.Constant(0);

            AssertNothingHappened(TurnRules.Tap(grid, 0, 0, 6, random));
            TestGrid.AssertRows(new[] { "Y" }, grid);
            Assert.Equal(0, random.Calls);
        }

        [Fact]
        public void Tap_ValidBlast_NoEmptyCellLeft()
        {
            Grid grid = TestGrid.Parse("BYGR", "RRGB", "YRBG");

            TurnRules.Tap(grid, 1, 1, 5, CountingRandom.Constant(4));

            AssertNoEmptyCell(grid);
        }

        [Fact]
        public void Tap_ValidBlast_UninvolvedColumnsUnchanged()
        {
            Grid grid = TestGrid.Parse("BYGR", "RRGB", "YRBG");

            TurnRules.Tap(grid, 1, 1, 5, CountingRandom.Constant(4));

            TestGrid.AssertRows(new[] { "PPGR", "BPGB", "YYBG" }, grid);
        }

        [Fact]
        public void Tap_BlastedList_MatchesBlastRulesOrder()
        {
            Grid grid = TestGrid.Filled(4, 4, TileColor.Green);
            Grid twin = Clone(grid);

            TurnResult result = TurnRules.Tap(grid, 2, 1, 6, CountingRandom.Cycle(6, 16));

            Assert.Equal(BlastRules.Blast(twin, 2, 1), result.Blasted);
        }

        [Fact]
        public void Tap_WholeGridOneColor_BlastsAllNoMovesRefillsAll()
        {
            Grid grid = TestGrid.Filled(4, 4, TileColor.Green);
            var random = CountingRandom.Cycle(6, 16);

            TurnResult result = TurnRules.Tap(grid, 2, 1, 6, random);

            Assert.Equal(16, result.Blasted.Count);
            Assert.Empty(result.Moves);
            Assert.Equal(16, random.Calls);
            TestGrid.AssertGroup(
                TestGrid.Pts(0, 0, 1, 0, 2, 0, 3, 0, 0, 1, 1, 1, 2, 1, 3, 1,
                             0, 2, 1, 2, 2, 2, 3, 2, 0, 3, 1, 3, 2, 3, 3, 3),
                result.Filled);
            TestGrid.AssertRows(new[] { "RGBY", "BYPO", "PORG", "RGBY" }, grid);
        }

        [Fact]
        public void Tap_TopRightCornerGroup_Accepted()
        {
            Grid grid = TestGrid.Parse("RGG", "RBG", "BRB");
            Grid twin = Clone(grid);
            var random = CountingRandom.Constant(3);

            TurnResult result = TurnRules.Tap(grid, 2, 2, 4, random);

            Assert.Equal(BlastRules.Blast(twin, 2, 2), result.Blasted);
            Assert.Equal(3, result.Blasted.Count);
            AssertMoves(new int[0], result.Moves);
            TestGrid.AssertRows(new[] { "RYY", "RBY", "BRB" }, grid);
        }

        [Fact]
        public void Tap_NullGrid_ThrowsArgumentNull()
        {
            var random = CountingRandom.Constant(0);
            Assert.Throws<ArgumentNullException>(() => TurnRules.Tap(null, 0, 0, 3, random));
            Assert.Equal(0, random.Calls);
        }

        [Fact]
        public void Tap_NullRandom_ThrowsGridUntouched()
        {
            Grid grid = TestGrid.Parse(Example3);
            Assert.Throws<ArgumentNullException>(() => TurnRules.Tap(grid, 1, 0, 3, null));
            TestGrid.AssertRows(Example3, grid);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(7)]
        public void Tap_ColorCountOutOfRangeOnGroup_ThrowsNothingBlasted(int colorCount)
        {
            Grid grid = TestGrid.Parse(Example3);
            var random = CountingRandom.Constant(0);

            Assert.Throws<ArgumentOutOfRangeException>(() => TurnRules.Tap(grid, 1, 0, colorCount, random));
            TestGrid.AssertRows(Example3, grid);
            Assert.Equal(0, random.Calls);
        }

        [Fact]
        public void Tap_ColorCountOutOfRangeOnSingleCell_StillThrows()
        {
            Grid grid = TestGrid.Parse(Example3);
            Assert.Throws<ArgumentOutOfRangeException>(
                () => TurnRules.Tap(grid, 0, 0, 0, CountingRandom.Constant(0)));
        }

        [Theory]
        [InlineData(-1, 0)]
        [InlineData(0, -1)]
        [InlineData(3, 0)]
        [InlineData(0, 3)]
        public void Tap_CoordinateOutside_ThrowsGridAndSourceUntouched(int x, int y)
        {
            Grid grid = TestGrid.Parse(Example3);
            var random = CountingRandom.Constant(0);

            Assert.Throws<ArgumentOutOfRangeException>(() => TurnRules.Tap(grid, x, y, 3, random));
            TestGrid.AssertRows(Example3, grid);
            Assert.Equal(0, random.Calls);
        }

        [Fact]
        public void Tap_NullGridAndEverythingElseBad_ThrowsArgumentNull()
        {
            Assert.Throws<ArgumentNullException>(() => TurnRules.Tap(null, -1, -1, 0, null));
        }

        [Fact]
        public void Tap_NullRandomBadCountBadCoordinate_RandomCheckedBeforeCount()
        {
            Grid grid = TestGrid.Parse(Example3);
            Assert.Throws<ArgumentNullException>(() => TurnRules.Tap(grid, 9, 9, 0, null));
            TestGrid.AssertRows(Example3, grid);
        }

        [Theory]
        [InlineData(3)]
        [InlineData(-1)]
        public void Tap_SourceReturnsOutOfRange_ThrowsLeavingBlastAndCollapse(int badValue)
        {
            Grid grid = TestGrid.Parse(Example3);

            Assert.Throws<InvalidOperationException>(
                () => TurnRules.Tap(grid, 1, 0, 3, CountingRandom.Constant(badValue)));
            TestGrid.AssertRows(new[] { "..G", "B.G", "BGB" }, grid);
        }

        [Fact]
        public void Tap_MissBeforeBlast_SameResultAsBlastOnly()
        {
            Grid a = TestGrid.Parse(Example3);
            Grid b = TestGrid.Parse(Example3);
            var randomA = new SeededRandom(7);
            var randomB = new SeededRandom(7);

            TurnRules.Tap(a, 0, 0, 3, randomA);
            TurnRules.Tap(a, 1, 0, 3, randomA);
            TurnRules.Tap(b, 1, 0, 3, randomB);

            Assert.Equal(TestGrid.Dump(b), TestGrid.Dump(a));
        }

        [Fact]
        public void Tap_Example4SameSeedSameTaps_IdenticalGridsAndResults()
        {
            var randomA = new SeededRandom(42);
            var randomB = new SeededRandom(42);
            Grid a = GridFactory.CreateRandom(8, 10, 5, randomA);
            Grid b = GridFactory.CreateRandom(8, 10, 5, randomB);
            int blasts = 0;

            for (int i = 0; i < 80; i++)
            {
                int x = i % 8;
                int y = i / 8;
                TurnResult ra = TurnRules.Tap(a, x, y, 5, randomA);
                TurnResult rb = TurnRules.Tap(b, x, y, 5, randomB);

                Assert.Equal(TestGrid.Dump(a), TestGrid.Dump(b));
                Assert.Equal(Format(ra.Blasted), Format(rb.Blasted));
                Assert.Equal(Format(ra.Moves), Format(rb.Moves));
                Assert.Equal(Format(ra.Filled), Format(rb.Filled));
                if (ra.HasBlasted)
                {
                    blasts++;
                }
            }

            Assert.True(blasts > 0, "The tap sequence never blasted, so the test proves nothing.");
        }

        [Fact]
        public void Tap_DifferentSeedsSameGrid_AtLeastOneCellDiffers()
        {
            Grid a = TestGrid.Filled(4, 4, TileColor.Red);
            Grid b = TestGrid.Filled(4, 4, TileColor.Red);

            TurnRules.Tap(a, 0, 0, 6, new SeededRandom(1));
            TurnRules.Tap(b, 0, 0, 6, new SeededRandom(2));

            Assert.NotEqual(TestGrid.Dump(a), TestGrid.Dump(b));
        }
    }
}
