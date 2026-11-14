using System.Collections.Generic;
using Xunit;
using static BlastGame.Core.Tests.TurnTestHelpers;

namespace BlastGame.Core.Tests
{
    /// <summary>Spec 07, gaps found by the reviewers: crate+anchor in one turn, balloon counts, chained second hit.</summary>
    public class ObstacleReviewFollowUpTests
    {
        private static readonly List<ColorGoal> NoColor = new List<ColorGoal>();

        private static List<ObstacleGoal> Obstacles(params ObstacleGoal[] goals) => new List<ObstacleGoal>(goals);

        [Fact]
        public void Tap_CrateUnderAnchorBreaks_AnchorFallsAndIsCollectedSameTurn()
        {
            Grid grid = ObstacleGrid.Parse(
                "A B",
                "C1 R",
                "R R");
            var session = new GameSession(grid, 3, 5, NoColor,
                Obstacles(new ObstacleGoal(ObstacleKind.Anchor, 1), new ObstacleGoal(ObstacleKind.Crate, 1)),
                CountingRandom.Constant(2));

            TurnResult result = session.Tap(0, 0);

            ObstacleGrid.AssertHits(result.Effects[0], "0,1 C1>.");
            AssertMoves(new[] { 0, 2, 0, 0 }, result.Collected);
            AssertMoves(new[] { 1, 2, 1, 0 }, result.Moves);
            Assert.Equal(0, session.Remaining(ObstacleKind.Anchor));
            Assert.Equal(0, session.Remaining(ObstacleKind.Crate));
            Assert.Equal(GameStatus.Won, session.Status);
            ObstacleGrid.AssertRows(grid,
                "B B",
                "B B",
                "B B");
        }

        [Fact]
        public void Tap_GroupWithTwoBalloons_CountsBoth()
        {
            Grid grid = ObstacleGrid.Parse("r r B");
            var session = new GameSession(grid, 3, 5, NoColor,
                Obstacles(new ObstacleGoal(ObstacleKind.WaterBalloon, 3)), CountingRandom.Constant(2));

            session.Tap(0, 0);

            Assert.Equal(1, session.Remaining(ObstacleKind.WaterBalloon));
        }

        [Fact]
        public void Tap_OrbOrb_CountsEveryBalloonOnBoard()
        {
            Grid grid = ObstacleGrid.Parse(
                "*R *B r",
                "b G R");
            var session = new GameSession(grid, 3, 5, NoColor,
                Obstacles(new ObstacleGoal(ObstacleKind.WaterBalloon, 3)), CountingRandom.Constant(2));

            TurnResult result = session.Tap(0, 1);

            Assert.Equal(EffectKind.OrbOrb, result.Effects[0].Kind);
            Assert.Equal(1, session.Remaining(ObstacleKind.WaterBalloon));
        }

        [Fact]
        public void Tap_OrbBombTriggersOldRocket_RocketGivesSecondHitAsSeparateBlast()
        {
            Grid grid = ObstacleGrid.Parse(
                "*R X G",
                "G C2 H",
                "B G R");

            TurnResult result = TurnRules.Tap(grid, 0, 2, 3, CountingRandom.Constant(0));

            Assert.Equal(new[] { "OrbBomb 0,2 t=-1", "Bomb 2,0 t=0", "Rocket 2,1 t=1" }, ComboHelpers.Heads(result.Effects));
            Assert.Equal(new[] { new string[0], new[] { "1,1 C2>C1" }, new[] { "1,1 C1>." } },
                ObstacleGrid.HitsPerEffect(result));
        }
    }
}
