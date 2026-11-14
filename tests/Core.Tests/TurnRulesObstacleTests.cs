using System.Collections.Generic;
using Xunit;
using static BlastGame.Core.Tests.TurnTestHelpers;

namespace BlastGame.Core.Tests
{
    /// <summary>Spec 07, TurnRules.Tap: obstacle cells, group hits, settle and fill order (Examples 1-4).</summary>
    public class TurnRulesObstacleTests
    {
        [Theory]
        [InlineData(1, 0)] // Pinata
        [InlineData(2, 0)] // Crate
        [InlineData(0, 1)] // Anchor
        public void Tap_ObstacleCell_DoesNothing(int x, int y)
        {
            Grid grid = ObstacleGrid.Parse(
                "A R R",
                "R P C2");
            Grid before = Clone(grid);
            var random = CountingRandom.Constant(0);

            TurnResult result = TurnRules.Tap(grid, x, y, 3, random);

            Assert.False(result.HasBlasted);
            Assert.Empty(result.Effects);
            Assert.Empty(result.Collected);
            Assert.Equal(0, random.Calls);
            SpecialGrid.AssertSameCells(before, grid);
        }

        [Fact]
        public void Tap_ObstacleBetweenSameColors_BreaksGroup()
        {
            Grid grid = ObstacleGrid.Parse("R P R");
            Grid before = Clone(grid);

            TurnResult result = TurnRules.Tap(grid, 0, 0, 3, CountingRandom.Constant(0));

            Assert.False(result.HasBlasted);
            SpecialGrid.AssertSameCells(before, grid);
        }

        [Fact]
        public void FindGroup_BalloonAndPlainSameColor_AreOneGroup()
        {
            Grid grid = ObstacleGrid.Parse("r R b", "G r R");

            var group = GroupFinder.FindGroup(grid, 1, 1);

            var set = new HashSet<CellPos>(group);
            Assert.Equal(4, group.Count);
            Assert.True(set.SetEquals(new[] { new CellPos(0, 1), new CellPos(1, 1), new CellPos(1, 0), new CellPos(2, 0) }),
                "Group was " + Format(group));
        }

        [Fact]
        public void Tap_Example1_PinataNextToTwoGroupCells_OneHit()
        {
            Grid grid = ObstacleGrid.Parse(
                "R P B B",
                "R R B G");
            var random = CountingRandom.Constant(1);

            TurnResult result = TurnRules.Tap(grid, 0, 0, 3, random);

            Assert.Single(result.Effects);
            ObstacleGrid.AssertHits(result.Effects[0], "1,1 P>.");
            Assert.Empty(result.Moves);
            Assert.Empty(result.Collected);
            TestGrid.AssertGroup(TestGrid.Pts(0, 0, 1, 0, 0, 1, 1, 1), result.Filled);
            Assert.Equal(4, random.Calls);
            ObstacleGrid.AssertRows(grid,
                "G G B B",
                "G G B G");
        }

        [Fact]
        public void Tap_CrateNextToTwoGroupCells_LosesOnlyOneLayer()
        {
            Grid grid = ObstacleGrid.Parse(
                "R C2",
                "R R");

            TurnResult result = TurnRules.Tap(grid, 0, 0, 3, CountingRandom.Constant(1));

            ObstacleGrid.AssertHits(result.Effects[0], "1,1 C2>C1");
            Assert.Equal("C1", ObstacleGrid.TokenOf(grid.Get(1, 1)));
        }

        [Fact]
        public void Tap_Example2_CrateLayerAndSegments()
        {
            Grid grid = ObstacleGrid.Parse(
                "B R B",
                "R C2 B",
                "R G B");
            var random = CountingRandom.Constant(0);

            TurnResult result = TurnRules.Tap(grid, 0, 0, 3, random);

            ObstacleGrid.AssertHits(result.Effects[0], "1,1 C2>C1");
            AssertMoves(new[] { 0, 2, 0, 0 }, result.Moves);
            Assert.Empty(result.Collected);
            TestGrid.AssertGroup(TestGrid.Pts(0, 1, 0, 2), result.Filled);
            Assert.Equal(2, random.Calls);
            ObstacleGrid.AssertRows(grid,
                "R R B",
                "R C1 B",
                "B G B");
        }

        [Fact]
        public void Tap_DiagonalObstacle_IsNotHit()
        {
            Grid grid = ObstacleGrid.Parse(
                "C2 G G",
                "B R R");

            TurnResult result = TurnRules.Tap(grid, 1, 0, 3, CountingRandom.Constant(0));

            Assert.True(result.HasBlasted);
            ObstacleGrid.AssertHits(result.Effects[0]);
            Assert.Equal("C2", ObstacleGrid.TokenOf(grid.Get(0, 1)));
        }

        [Fact]
        public void Tap_CornerCrate_IsHitByGroup()
        {
            Grid grid = ObstacleGrid.Parse(
                "R R G",
                "C2 R G");

            TurnResult result = TurnRules.Tap(grid, 1, 0, 3, CountingRandom.Constant(0));

            ObstacleGrid.AssertHits(result.Effects[0], "0,0 C2>C1");
        }

        [Fact]
        public void Tap_GroupHitsTwoObstacles_HitsOrderedByYThenX()
        {
            Grid grid = ObstacleGrid.Parse(
                "P R R",
                "R R C1");

            TurnResult result = TurnRules.Tap(grid, 1, 0, 3, CountingRandom.Constant(0));

            ObstacleGrid.AssertHits(result.Effects[0], "2,0 C1>.", "0,1 P>.");
        }

        [Fact]
        public void Tap_NeighborBalloonAndAnchor_AreNotAffected()
        {
            Grid grid = ObstacleGrid.Parse(
                "A B",
                "R R",
                "g B");

            TurnResult result = TurnRules.Tap(grid, 0, 1, 3, CountingRandom.Constant(1));

            ObstacleGrid.AssertHits(result.Effects[0]);
            Assert.Equal("g", ObstacleGrid.TokenOf(grid.Get(0, 0)));
            foreach (ClearedCell c in result.Effects[0].Cleared)
            {
                Assert.NotEqual(new CellPos(0, 0), c.Pos);
                Assert.NotEqual(new CellPos(0, 2), c.Pos);
            }
        }

        [Fact]
        public void Tap_CrateThreeTimes_ThreeTwoOneGoneThenSegmentsMerge()
        {
            Grid grid = ObstacleGrid.Parse(
                "B R",
                "B R",
                "B R",
                "C3 R");
            var random = CountingRandom.Constant(0);

            TurnResult first = TurnRules.Tap(grid, 1, 0, 3, random);
            TurnResult second = TurnRules.Tap(grid, 1, 0, 3, random);
            TurnResult third = TurnRules.Tap(grid, 1, 0, 3, random);

            ObstacleGrid.AssertHits(first.Effects[0], "0,0 C3>C2");
            ObstacleGrid.AssertHits(second.Effects[0], "0,0 C2>C1");
            ObstacleGrid.AssertHits(third.Effects[0], "0,0 C1>.");
            AssertMoves(new[] { 0, 1, 0, 0, 0, 2, 0, 1, 0, 3, 0, 2 }, third.Moves);
            TestGrid.AssertGroup(TestGrid.Pts(1, 0, 1, 1, 1, 2, 0, 3, 1, 3), third.Filled);
            ObstacleGrid.AssertRows(grid,
                "R R",
                "B R",
                "B R",
                "B R");
        }

        [Fact]
        public void Tap_Example4_AnchorFallsToBottomAndIsCollected()
        {
            Grid grid = ObstacleGrid.Parse(
                "A B",
                "R B",
                "R G");
            var random = CountingRandom.Constant(2);

            TurnResult result = TurnRules.Tap(grid, 0, 0, 3, random);

            AssertMoves(new[] { 0, 2, 0, 0 }, result.Collected);
            Assert.Empty(result.Moves);
            ObstacleGrid.AssertHits(result.Effects[0]);
            TestGrid.AssertGroup(TestGrid.Pts(0, 0, 0, 1, 0, 2), result.Filled);
            Assert.Equal(3, random.Calls);
            ObstacleGrid.AssertRows(grid,
                "B B",
                "B B",
                "B G");
        }

        [Fact]
        public void Tap_Example5_BalloonPopsWithItsSquare()
        {
            Grid grid = ObstacleGrid.Parse("r R B");

            TurnResult result = TurnRules.Tap(grid, 1, 0, 3, CountingRandom.Constant(0));

            var cleared = new HashSet<ClearedCell>(result.Effects[0].Cleared);
            Assert.Equal(2, cleared.Count);
            Assert.Contains(new ClearedCell(new CellPos(0, 0), Cell.OfColor(TileColor.Red).WithBalloon(true)), cleared);
            Assert.Contains(new ClearedCell(new CellPos(1, 0), Cell.OfColor(TileColor.Red)), cleared);
            ObstacleGrid.AssertHits(result.Effects[0]);
            ObstacleGrid.AssertRows(grid, "R R B");
        }

        [Fact]
        public void Tap_BalloonOnTappedCellOfBigGroup_PopsAndSpecialIsPlainRocket()
        {
            Grid grid = ObstacleGrid.Parse("r R R R R G");

            TurnResult result = TurnRules.Tap(grid, 0, 0, 3, ObstacleGrid.ThenZeros(0));

            Assert.Contains(new ClearedCell(new CellPos(0, 0), Cell.OfColor(TileColor.Red).WithBalloon(true)),
                result.Effects[0].Cleared);
            Assert.True(grid.Get(0, 0) == Cell.OfSpecial(SpecialKind.RocketHorizontal));
            Assert.False(grid.Get(0, 0).HasBalloon);
        }

        [Fact]
        public void Tap_ExampleThree_CrateOutsideRocketLeavesGapUnfilled()
        {
            Grid grid = ObstacleGrid.Parse(
                "G R",
                "C1 B",
                "R H");
            var random = CountingRandom.Constant(0);

            TurnResult result = TurnRules.Tap(grid, 1, 0, 3, random);

            ObstacleGrid.AssertHits(result.Effects[0]);
            AssertMoves(new[] { 1, 1, 1, 0, 1, 2, 1, 1 }, result.Moves);
            TestGrid.AssertGroup(TestGrid.Pts(1, 2), result.Filled);
            Assert.Equal(1, random.Calls);
            ObstacleGrid.AssertRows(grid,
                "G R",
                "C1 R",
                ". B");
        }

        [Fact]
        public void Tap_ExampleThree_CrateInBlastAreaBreaksAndColumnSettles()
        {
            Grid grid = ObstacleGrid.Parse(
                "G R",
                "C1 B",
                "R X");
            var random = CountingRandom.Constant(0);

            TurnResult result = TurnRules.Tap(grid, 1, 0, 3, random);

            Assert.Equal("1,0X 0,0R 1,1B", ObstacleGrid.Cleared(result.Effects[0]));
            ObstacleGrid.AssertHits(result.Effects[0], "0,1 C1>.");
            AssertMoves(new[] { 0, 2, 0, 0, 1, 2, 1, 0 }, result.Moves);
            TestGrid.AssertGroup(TestGrid.Pts(0, 1, 1, 1, 0, 2, 1, 2), result.Filled);
            ObstacleGrid.AssertRows(grid,
                "R R",
                "R R",
                "G R");
        }
    }
}
