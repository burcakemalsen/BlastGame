using System;
using Xunit;
using static BlastGame.Core.Tests.TurnTestHelpers;

namespace BlastGame.Core.Tests
{
    /// <summary>Spec 07, GravityRules.Settle / Collapse with Crates and Anchors.</summary>
    public class GravitySettleTests
    {
        [Fact]
        public void Settle_NullGrid_ThrowsArgumentNull()
        {
            var ex = Assert.Throws<ArgumentNullException>(() => GravityRules.Settle(null));
            Assert.Equal("grid", ex.ParamName);
        }

        [Fact]
        public void Settle_CellsOnCrate_StayOnCrateAndGapBelowStays()
        {
            Grid grid = ObstacleGrid.Parse("G", "C1", ".");

            GravityResult result = GravityRules.Settle(grid);

            Assert.Empty(result.Moves);
            Assert.Empty(result.Collected);
            ObstacleGrid.AssertRows(grid, "G", "C1", ".");
        }

        [Fact]
        public void Settle_TwoCratesInColumn_EachSegmentFallsToItsOwnFloor()
        {
            // bottom-up: . C1 . R C2 . G
            Grid grid = ObstacleGrid.Parse("G", ".", "C2", "R", ".", "C1", ".");

            GravityResult result = GravityRules.Settle(grid);

            AssertMoves(new[] { 0, 3, 0, 2, 0, 6, 0, 5 }, result.Moves);
            Assert.Empty(result.Collected);
            ObstacleGrid.AssertRows(grid, ".", "G", "C2", ".", "R", "C1", ".");
        }

        [Fact]
        public void Settle_PinataAndBalloonSquare_FallLikeSquares()
        {
            Grid grid = ObstacleGrid.Parse("r", "P", ".", ".");

            GravityResult result = GravityRules.Settle(grid);

            AssertMoves(new[] { 0, 2, 0, 0, 0, 3, 0, 1 }, result.Moves);
            ObstacleGrid.AssertRows(grid, ".", ".", "r", "P");
        }

        [Fact]
        public void Settle_AnchorReachesBottom_IsCollectedAndCellsAboveTakeItsPlace()
        {
            Grid grid = ObstacleGrid.Parse("R", "A", ".");

            GravityResult result = GravityRules.Settle(grid);

            AssertMoves(new[] { 0, 1, 0, 0 }, result.Collected);
            AssertMoves(new[] { 0, 2, 0, 0 }, result.Moves);
            ObstacleGrid.AssertRows(grid, ".", ".", "R");
        }

        [Fact]
        public void Settle_AnchorAlreadyAtBottom_IsCollectedWithSameFromAndTo()
        {
            Grid grid = ObstacleGrid.Parse("R", "A");

            GravityResult result = GravityRules.Settle(grid);

            AssertMoves(new[] { 0, 0, 0, 0 }, result.Collected);
            AssertMoves(new[] { 0, 1, 0, 0 }, result.Moves);
            ObstacleGrid.AssertRows(grid, ".", "R");
        }

        [Fact]
        public void Settle_LeadingAnchorsWithGap_AreCollectedTogether()
        {
            // bottom-up: A . A R -> both anchors lead the falling order
            Grid grid = ObstacleGrid.Parse("R", "A", ".", "A");

            GravityResult result = GravityRules.Settle(grid);

            AssertMoves(new[] { 0, 0, 0, 0, 0, 2, 0, 0 }, result.Collected);
            AssertMoves(new[] { 0, 3, 0, 0 }, result.Moves);
            ObstacleGrid.AssertRows(grid, ".", ".", ".", "R");
        }

        [Fact]
        public void Settle_AnchorAboveSquare_IsNotCollected()
        {
            Grid grid = ObstacleGrid.Parse("A", "R", ".");

            GravityResult result = GravityRules.Settle(grid);

            Assert.Empty(result.Collected);
            AssertMoves(new[] { 0, 1, 0, 0, 0, 2, 0, 1 }, result.Moves);
            ObstacleGrid.AssertRows(grid, ".", "A", "R");
        }

        [Fact]
        public void Settle_AnchorAboveCrate_FallsOntoCrateAndIsNotCollected()
        {
            // bottom-up: . C1 . A
            Grid grid = ObstacleGrid.Parse("A", ".", "C1", ".");

            GravityResult result = GravityRules.Settle(grid);

            Assert.Empty(result.Collected);
            AssertMoves(new[] { 0, 3, 0, 2 }, result.Moves);
            ObstacleGrid.AssertRows(grid, ".", "A", "C1", ".");
        }

        [Fact]
        public void Settle_TwoColumns_CollectedAndMovesInContractOrder()
        {
            // col0 bottom-up: . A A R ; col1 bottom-up: A G . B
            Grid grid = ObstacleGrid.Parse(
                "R B",
                "A .",
                "A G",
                ". A");

            GravityResult result = GravityRules.Settle(grid);

            AssertMoves(new[] { 0, 1, 0, 0, 0, 2, 0, 0, 1, 0, 1, 0 }, result.Collected);
            AssertMoves(new[] { 0, 3, 0, 0, 1, 1, 1, 0, 1, 3, 1, 1 }, result.Moves);
            ObstacleGrid.AssertRows(grid,
                ". .",
                ". .",
                ". B",
                "R G");
        }

        [Fact]
        public void Settle_NoObstacles_SameMovesAsCollapse()
        {
            Grid a = TestGrid.Parse("RG.", ".BG", "R.B", "..Y");
            Grid b = Clone(a);

            GravityResult settled = GravityRules.Settle(a);
            var collapsed = GravityRules.Collapse(b);

            Assert.Equal(Format(collapsed), Format(settled.Moves));
            Assert.Empty(settled.Collected);
            TestGrid.AssertRows(TestGrid.Dump(b), a);
        }

        [Fact]
        public void Collapse_Crate_RespectsSegments()
        {
            Grid grid = ObstacleGrid.Parse("R", ".", "C2", "G", ".");

            var moves = GravityRules.Collapse(grid);

            AssertMoves(new[] { 0, 1, 0, 0, 0, 4, 0, 3 }, moves);
            ObstacleGrid.AssertRows(grid, ".", "R", "C2", ".", "G");
        }

        [Fact]
        public void Collapse_AnchorAtBottom_IsNotCollected()
        {
            Grid grid = ObstacleGrid.Parse("A", ".");

            var moves = GravityRules.Collapse(grid);

            AssertMoves(new[] { 0, 1, 0, 0 }, moves);
            ObstacleGrid.AssertRows(grid, ".", "A");
        }
    }

    /// <summary>Spec 07, RefillRules.Fill with Crates.</summary>
    public class RefillCrateTests
    {
        [Fact]
        public void Fill_GapBelowCrate_IsNotFilled()
        {
            Grid grid = ObstacleGrid.Parse(".", "C1", ".");
            var random = CountingRandom.Constant(1);

            var filled = RefillRules.Fill(grid, 3, random);

            TestGrid.AssertGroup(TestGrid.Pts(0, 2), filled);
            Assert.Equal(1, random.Calls);
            ObstacleGrid.AssertRows(grid, "G", "C1", ".");
        }

        [Fact]
        public void Fill_TwoCrates_OnlyAboveTopCrate()
        {
            Grid grid = ObstacleGrid.Parse(".", "C2", ".", "C1", ".");
            var random = CountingRandom.Constant(0);

            var filled = RefillRules.Fill(grid, 3, random);

            TestGrid.AssertGroup(TestGrid.Pts(0, 4), filled);
            Assert.Equal(1, random.Calls);
            ObstacleGrid.AssertRows(grid, "R", "C2", ".", "C1", ".");
        }

        [Fact]
        public void Fill_MixedColumns_DrawsInRowOrderSkippingUnfilledCells()
        {
            // col0 has a crate at y=1 with gaps above and below; col1 is empty.
            Grid grid = ObstacleGrid.Parse(
                ". .",
                "C1 .",
                ". .");
            var random = new CountingRandom(0, 1, 2, 0);

            var filled = RefillRules.Fill(grid, 3, random);

            TestGrid.AssertGroup(TestGrid.Pts(1, 0, 1, 1, 0, 2, 1, 2), filled);
            Assert.Equal(4, random.Calls);
            ObstacleGrid.AssertRows(grid,
                "B R",
                "C1 G",
                ". R");
        }

        [Fact]
        public void Fill_PinataAndAnchorDoNotBlock_AllGapsFilledWithPlainSquares()
        {
            Grid grid = ObstacleGrid.Parse(".", "A", ".", "P", ".");
            var random = CountingRandom.Constant(2);

            var filled = RefillRules.Fill(grid, 3, random);

            TestGrid.AssertGroup(TestGrid.Pts(0, 0, 0, 2, 0, 4), filled);
            ObstacleGrid.AssertRows(grid, "B", "A", "B", "P", "B");
            Assert.False(grid.Get(0, 0).HasBalloon);
        }
    }
}
