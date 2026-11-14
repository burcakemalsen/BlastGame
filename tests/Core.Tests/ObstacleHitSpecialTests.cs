using Xunit;
using static BlastGame.Core.Tests.TurnTestHelpers;

namespace BlastGame.Core.Tests
{
    /// <summary>Spec 07: hits from single specials, combos and chains; balloons in special areas.</summary>
    public class ObstacleHitSpecialTests
    {
        [Fact]
        public void Tap_Example6_RocketPassesObstaclesToRowEnd()
        {
            Grid grid = ObstacleGrid.Parse(
                "H R C1 P A B",
                "G B G B Y G");
            var random = CountingRandom.Constant(0);

            TurnResult result = TurnRules.Tap(grid, 0, 1, 3, random);

            Assert.Single(result.Effects);
            Assert.Equal("0,1H 1,1R 5,1B", ObstacleGrid.Cleared(result.Effects[0]));
            ObstacleGrid.AssertHits(result.Effects[0], "2,1 C1>.", "3,1 P>.");
            Assert.Empty(result.Moves);
            Assert.Empty(result.Collected);
            TestGrid.AssertGroup(TestGrid.Pts(0, 1, 1, 1, 2, 1, 3, 1, 5, 1), result.Filled);
            ObstacleGrid.AssertRows(grid,
                "R R R R A R",
                "G B G B Y G");
        }

        [Fact]
        public void Tap_VerticalRocketThroughThreeLayerCrate_CrateDropsToTwo()
        {
            Grid grid = ObstacleGrid.Parse("V", "C3", "R");

            TurnResult result = TurnRules.Tap(grid, 0, 2, 3, CountingRandom.Constant(0));

            Assert.Equal("0,2V 0,0R", ObstacleGrid.Cleared(result.Effects[0]));
            ObstacleGrid.AssertHits(result.Effects[0], "0,1 C3>C2");
            Assert.Equal("C2", ObstacleGrid.TokenOf(grid.Get(0, 1)));
        }

        [Fact]
        public void Tap_Rocket_DoesNotHitObstaclesBesideItsLine()
        {
            Grid grid = ObstacleGrid.Parse(
                "P C2 G",
                "H R R");

            TurnResult result = TurnRules.Tap(grid, 0, 0, 3, CountingRandom.Constant(0));

            ObstacleGrid.AssertHits(result.Effects[0]);
            Assert.Equal("C2", ObstacleGrid.TokenOf(grid.Get(1, 1)));
            Assert.Equal("P", ObstacleGrid.TokenOf(grid.Get(0, 0)));
        }

        [Fact]
        public void Tap_Bomb_HitsObstaclesInAreaOnlyAndLeavesAnchor()
        {
            Grid grid = ObstacleGrid.Parse(
                "G A B G",
                "B X C2 G",
                "P G R C3");

            TurnResult result = TurnRules.Tap(grid, 1, 1, 3, CountingRandom.Constant(0));

            Assert.Single(result.Effects);
            Assert.Equal("1,1X 1,0G 2,0R 0,1B 0,2G 2,2B", ObstacleGrid.Cleared(result.Effects[0]));
            ObstacleGrid.AssertHits(result.Effects[0], "0,0 P>.", "2,1 C2>C1");
            AssertMoves(new[] { 1, 2, 1, 0 }, result.Collected);
            Assert.Equal("C3", ObstacleGrid.TokenOf(grid.Get(3, 0)));
            Assert.Equal("C1", ObstacleGrid.TokenOf(grid.Get(2, 1)));
            Assert.True(grid.Get(2, 0).IsEmpty);
        }

        [Fact]
        public void Tap_Orb_HitsNeighborsOfClearedSquaresOncePopsBalloon()
        {
            Grid grid = ObstacleGrid.Parse(
                "P R C2 G",
                "R G r *R");

            TurnResult result = TurnRules.Tap(grid, 3, 0, 3, CountingRandom.Constant(1));

            Assert.Single(result.Effects);
            Assert.Equal("3,0*R 0,0R 2,0r 1,1R", ObstacleGrid.Cleared(result.Effects[0]));
            ObstacleGrid.AssertHits(result.Effects[0], "0,1 P>.", "2,1 C2>C1");
        }

        [Fact]
        public void Tap_RocketOverBalloon_PopsBalloon()
        {
            Grid grid = ObstacleGrid.Parse("H r B");

            TurnResult result = TurnRules.Tap(grid, 0, 0, 3, CountingRandom.Constant(0));

            Assert.Equal("0,0H 1,0r 2,0B", ObstacleGrid.Cleared(result.Effects[0]));
        }

        [Fact]
        public void Tap_RocketCross_HitsObstaclesInCrossOnly()
        {
            Grid grid = ObstacleGrid.Parse(
                "G C2 C1 G",
                "R H V P",
                "B G B G");

            TurnResult result = TurnRules.Tap(grid, 1, 1, 3, CountingRandom.Constant(0));

            Assert.Single(result.Effects);
            Assert.Equal(EffectKind.RocketCross, result.Effects[0].Kind);
            Assert.Equal("1,1H 2,1V 1,0G 0,1R", ObstacleGrid.Cleared(result.Effects[0]));
            ObstacleGrid.AssertHits(result.Effects[0], "3,1 P>.", "1,2 C2>C1");
            Assert.Equal("C1", ObstacleGrid.TokenOf(grid.Get(2, 2)));
        }

        [Fact]
        public void Tap_BigBomb_HitsUpToThreeCellsAway()
        {
            Grid grid = ObstacleGrid.Parse("X X R C2 P G");

            TurnResult result = TurnRules.Tap(grid, 0, 0, 3, CountingRandom.Constant(0));

            Assert.Equal(EffectKind.BigBomb, result.Effects[0].Kind);
            Assert.Equal("0,0X 1,0X 2,0R", ObstacleGrid.Cleared(result.Effects[0]));
            ObstacleGrid.AssertHits(result.Effects[0], "3,0 C2>C1");
            Assert.Equal("P", ObstacleGrid.TokenOf(grid.Get(4, 0)));
        }

        [Fact]
        public void Tap_OrbOrb_HitsEveryObstacleOnceAnchorFallsAndIsCollected()
        {
            Grid grid = ObstacleGrid.Parse(
                "C3 P A B",
                "*R *B R G");
            var random = CountingRandom.Constant(0);

            TurnResult result = TurnRules.Tap(grid, 0, 0, 3, random);

            Assert.Single(result.Effects);
            Assert.Equal(EffectKind.OrbOrb, result.Effects[0].Kind);
            Assert.Equal("0,0*R 1,0*B 2,0R 3,0G 3,1B", ObstacleGrid.Cleared(result.Effects[0]));
            ObstacleGrid.AssertHits(result.Effects[0], "0,1 C3>C2", "1,1 P>.");
            AssertMoves(new[] { 2, 1, 2, 0 }, result.Collected);
            Assert.Empty(result.Moves);
            TestGrid.AssertGroup(TestGrid.Pts(1, 0, 2, 0, 3, 0, 1, 1, 2, 1, 3, 1), result.Filled);
            Assert.Equal(6, random.Calls);
            ObstacleGrid.AssertRows(grid,
                "C2 R R R",
                ". R R R");
        }

        [Fact]
        public void Tap_OrbRocketOverlappingRockets_CrateHitOnceInFirstRocketEffect()
        {
            Grid grid = ObstacleGrid.Parse(
                "B *R H",
                "C2 B R",
                "R G B");

            TurnResult result = TurnRules.Tap(grid, 1, 2, 3, ObstacleGrid.ThenZeros(1, 0));

            Assert.Equal(3, result.Effects.Count);
            Assert.Equal("0,0R 2,1R", ObstacleGrid.Converted(result.Effects[0]));
            Assert.Equal("0,0V 0,2B", ObstacleGrid.Cleared(result.Effects[1]));
            Assert.Equal("2,1H 1,1B", ObstacleGrid.Cleared(result.Effects[2]));
            Assert.Equal(new[] { new string[0], new[] { "0,1 C2>C1" }, new string[0] }, ObstacleGrid.HitsPerEffect(result));
            Assert.Equal("C1", ObstacleGrid.TokenOf(grid.Get(0, 1)));
        }

        [Fact]
        public void Tap_OrbBombOverlappingBombs_CrateHitOnceInFirstBombEffect()
        {
            Grid grid = ObstacleGrid.Parse(
                "*R X G",
                "G C2 G",
                "R G R");

            TurnResult result = TurnRules.Tap(grid, 0, 2, 3, CountingRandom.Constant(0));

            Assert.Equal(3, result.Effects.Count);
            Assert.Equal("0,0X 1,0G 0,1G", ObstacleGrid.Cleared(result.Effects[1]));
            Assert.Equal("2,0X 2,1G", ObstacleGrid.Cleared(result.Effects[2]));
            Assert.Equal(new[] { new string[0], new[] { "1,1 C2>C1" }, new string[0] }, ObstacleGrid.HitsPerEffect(result));
            Assert.Equal("C1", ObstacleGrid.TokenOf(grid.Get(1, 1)));
        }

        [Theory]
        [InlineData("C2", "0,2 C2>C1", "0,2 C1>.")]
        [InlineData("P", "0,2 P>.", null)]
        public void Tap_BombThenChainedRocket_SecondHitOnlyIfObstacleStillThere(string obstacle, string firstHit, string secondHit)
        {
            Grid grid = ObstacleGrid.Parse(
                obstacle + " G H",
                "B X G",
                "G B G");

            TurnResult result = TurnRules.Tap(grid, 1, 1, 3, CountingRandom.Constant(0));

            Assert.Equal(2, result.Effects.Count);
            Assert.Equal(0, result.Effects[1].TriggeredBy);
            Assert.Equal("1,1X 0,0G 1,0B 2,0G 0,1B 2,1G 1,2G", ObstacleGrid.Cleared(result.Effects[0]));
            Assert.Equal("2,2H", ObstacleGrid.Cleared(result.Effects[1]));
            ObstacleGrid.AssertHits(result.Effects[0], firstHit);
            if (secondHit == null)
            {
                ObstacleGrid.AssertHits(result.Effects[1]);
            }
            else
            {
                ObstacleGrid.AssertHits(result.Effects[1], secondHit);
            }
        }

        [Fact]
        public void Tap_ObstacleBetweenSpecials_NoComboRocketHitsAndChains()
        {
            Grid grid = ObstacleGrid.Parse("H P V");

            TurnResult result = TurnRules.Tap(grid, 0, 0, 3, CountingRandom.Constant(0));

            Assert.Equal(new[] { "Rocket 0,0 t=-1", "Rocket 2,0 t=0" }, ComboHelpers.Heads(result.Effects));
            Assert.Equal(1, result.Effects[0].MergedCount);
            ObstacleGrid.AssertHits(result.Effects[0], "1,0 P>.");
            ObstacleGrid.AssertHits(result.Effects[1]);
        }

        [Fact]
        public void Tap_OrbRocketConvertsBalloonSquare_ConvertedHasBalloonNewRocketDoesNot()
        {
            Grid grid = ObstacleGrid.Parse(
                "*R H B",
                "r G B");

            TurnResult result = TurnRules.Tap(grid, 0, 1, 3, CountingRandom.Constant(0));

            Assert.Equal("0,0r", ObstacleGrid.Converted(result.Effects[0]));
            Assert.Equal("0,0H 1,0G 2,0B", ObstacleGrid.Cleared(result.Effects[1]));
            Assert.False(result.Effects[1].Cleared[0].Cell.HasBalloon);
        }
    }
}
