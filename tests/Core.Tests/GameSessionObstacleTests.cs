using System;
using System.Collections.Generic;
using Xunit;

namespace BlastGame.Core.Tests
{
    /// <summary>Spec 07, GameSession: obstacle goals, constructor checks, counting, win/lose.</summary>
    public class GameSessionObstacleTests
    {
        private static readonly List<ColorGoal> NoColor = new List<ColorGoal>();
        private static readonly List<ObstacleGoal> NoObstacle = new List<ObstacleGoal>();

        private static List<ColorGoal> Colors(params ColorGoal[] goals) => new List<ColorGoal>(goals);

        private static List<ObstacleGoal> Obstacles(params ObstacleGoal[] goals) => new List<ObstacleGoal>(goals);

        private static ObstacleGoal O(ObstacleKind kind, int count) => new ObstacleGoal(kind, count);

        private static ColorGoal C(TileColor color, int count) => new ColorGoal(color, count);

        private static Grid Board() => TestGrid.Parse("RGB", "BRG", "GBR");

        // ---- Constructor ----

        [Fact]
        public void Constructor_ObstacleGoals_KeptInGivenOrderWithRemaining()
        {
            var session = new GameSession(Board(), 3, 5, Colors(C(TileColor.Red, 2)),
                Obstacles(O(ObstacleKind.Anchor, 1), O(ObstacleKind.Pinata, 3)), CountingRandom.Constant(0));

            Assert.Equal(new[] { O(ObstacleKind.Anchor, 1), O(ObstacleKind.Pinata, 3) }, session.ObstacleGoals);
            Assert.Equal(1, session.Remaining(ObstacleKind.Anchor));
            Assert.Equal(3, session.Remaining(ObstacleKind.Pinata));
            Assert.Equal(2, session.Remaining(TileColor.Red));
        }

        [Fact]
        public void Constructor_OnlyObstacleGoals_IsValid()
        {
            var session = new GameSession(Board(), 3, 5, NoColor, Obstacles(O(ObstacleKind.Crate, 2)), CountingRandom.Constant(0));

            Assert.Empty(session.Goals);
            Assert.Equal(2, session.Remaining(ObstacleKind.Crate));
            Assert.Equal(GameStatus.Playing, session.Status);
        }

        [Fact]
        public void Constructor_FiveParameters_HasNoObstacleGoals()
        {
            var session = new GameSession(Board(), 3, 5, Colors(C(TileColor.Red, 2)), CountingRandom.Constant(0));

            Assert.NotNull(session.ObstacleGoals);
            Assert.Empty(session.ObstacleGoals);
        }

        [Fact]
        public void Constructor_NullObstacleGoals_ThrowsArgumentNull()
        {
            var ex = Assert.Throws<ArgumentNullException>(() =>
                new GameSession(Board(), 3, 5, Colors(C(TileColor.Red, 2)), null, null));
            Assert.Equal("obstacleGoals", ex.ParamName);
        }

        [Fact]
        public void Constructor_NullGoalsAndNullObstacleGoals_GoalsFirst()
        {
            var ex = Assert.Throws<ArgumentNullException>(() =>
                new GameSession(Board(), 3, 5, null, null, CountingRandom.Constant(0)));
            Assert.Equal("goals", ex.ParamName);
        }

        [Fact]
        public void Constructor_BadColorGoalAndNullObstacleGoals_ColorRuleFirst()
        {
            var ex = Assert.Throws<ArgumentException>(() =>
                new GameSession(Board(), 3, 5, Colors(C(TileColor.Red, 1), C(TileColor.Red, 2)), null, CountingRandom.Constant(0)));
            Assert.Equal("goals", ex.ParamName);
        }

        [Fact]
        public void Constructor_BothGoalListsEmpty_ThrowsArgumentExceptionGoalsBeforeRandom()
        {
            var ex = Assert.Throws<ArgumentException>(() => new GameSession(Board(), 3, 5, NoColor, NoObstacle, null));
            Assert.Equal("goals", ex.ParamName);
        }

        public static TheoryData<ObstacleGoal[]> BadObstacleGoals => new TheoryData<ObstacleGoal[]>
        {
            new[] { O(ObstacleKind.Pinata, 1), O(ObstacleKind.Pinata, 2) },
            new[] { O(ObstacleKind.Anchor, 1), O(ObstacleKind.Crate, 0) },
        };

        [Theory]
        [MemberData(nameof(BadObstacleGoals))]
        public void Constructor_BadObstacleGoals_ThrowsArgumentExceptionBeforeRandom(ObstacleGoal[] goals)
        {
            var ex = Assert.Throws<ArgumentException>(() =>
                new GameSession(Board(), 3, 5, NoColor, Obstacles(goals), null));
            Assert.Equal("obstacleGoals", ex.ParamName);
        }

        [Fact]
        public void Constructor_ValidGoalsNullRandom_ThrowsArgumentNull()
        {
            var ex = Assert.Throws<ArgumentNullException>(() =>
                new GameSession(Board(), 3, 5, NoColor, Obstacles(O(ObstacleKind.Crate, 1)), null));
            Assert.Equal("random", ex.ParamName);
        }

        [Theory]
        [InlineData(ObstacleKind.Anchor)]
        [InlineData((ObstacleKind)42)]
        public void Remaining_KindNotAGoal_ThrowsArgumentException(ObstacleKind kind)
        {
            var session = new GameSession(Board(), 3, 5, NoColor, Obstacles(O(ObstacleKind.Pinata, 1)), CountingRandom.Constant(0));

            var ex = Assert.Throws<ArgumentException>(() => session.Remaining(kind));
            Assert.Equal("kind", ex.ParamName);
        }

        // ---- Counting ----

        [Fact]
        public void Tap_Example1_PinataGoalCountsBrokenPinata()
        {
            Grid grid = ObstacleGrid.Parse("R P B B", "R R B G");
            var session = new GameSession(grid, 3, 5, NoColor, Obstacles(O(ObstacleKind.Pinata, 2)), CountingRandom.Constant(1));

            session.Tap(0, 0);

            Assert.Equal(1, session.Remaining(ObstacleKind.Pinata));
            Assert.Equal(4, session.MovesLeft);
        }

        [Fact]
        public void Tap_CrateGoal_CountsOnlyLastLayerThenWins()
        {
            Grid grid = ObstacleGrid.Parse("B R", "B R", "B R", "C3 R");
            var session = new GameSession(grid, 3, 10, NoColor, Obstacles(O(ObstacleKind.Crate, 1)), CountingRandom.Constant(0));

            session.Tap(1, 0);
            Assert.Equal(1, session.Remaining(ObstacleKind.Crate));
            session.Tap(1, 0);
            Assert.Equal(1, session.Remaining(ObstacleKind.Crate));
            Assert.Equal(GameStatus.Playing, session.Status);
            session.Tap(1, 0);

            Assert.Equal(0, session.Remaining(ObstacleKind.Crate));
            Assert.Equal(GameStatus.Won, session.Status);
        }

        [Fact]
        public void Tap_Example5_BalloonAndColorCountedTogether()
        {
            Grid grid = ObstacleGrid.Parse("r R B");
            var session = new GameSession(grid, 3, 5, Colors(C(TileColor.Red, 5)),
                Obstacles(O(ObstacleKind.WaterBalloon, 2)), CountingRandom.Constant(0));

            session.Tap(1, 0);

            Assert.Equal(3, session.Remaining(TileColor.Red));
            Assert.Equal(1, session.Remaining(ObstacleKind.WaterBalloon));
        }

        [Fact]
        public void Tap_Example4_AnchorGoalCountsCollected()
        {
            Grid grid = ObstacleGrid.Parse("A B", "R B", "R G");
            var session = new GameSession(grid, 3, 5, NoColor, Obstacles(O(ObstacleKind.Anchor, 2)), CountingRandom.Constant(2));

            session.Tap(0, 0);

            Assert.Equal(1, session.Remaining(ObstacleKind.Anchor));
        }

        [Fact]
        public void Tap_ConvertedBalloon_CountedOnceAsBalloonAndAsColor()
        {
            Grid grid = ObstacleGrid.Parse("*R H B", "r G B");
            var session = new GameSession(grid, 3, 5, Colors(C(TileColor.Red, 3)),
                Obstacles(O(ObstacleKind.WaterBalloon, 2)), CountingRandom.Constant(0));

            session.Tap(0, 1);

            Assert.Equal(1, session.Remaining(ObstacleKind.WaterBalloon));
            Assert.Equal(2, session.Remaining(TileColor.Red));
        }

        [Fact]
        public void Tap_MorePinatasThanGoal_StopsAtZeroAndColorGoalKeepsPlaying()
        {
            Grid grid = ObstacleGrid.Parse("P R P", "G R R");
            var session = new GameSession(grid, 3, 5, Colors(C(TileColor.Green, 5)),
                Obstacles(O(ObstacleKind.Pinata, 1)), CountingRandom.Constant(2));

            session.Tap(1, 0);

            Assert.Equal(0, session.Remaining(ObstacleKind.Pinata));
            Assert.Equal(GameStatus.Playing, session.Status);
        }

        [Fact]
        public void Tap_ObstacleCell_SpendsNoMove()
        {
            Grid grid = ObstacleGrid.Parse("R P B B", "R R B G");
            var random = CountingRandom.Constant(1);
            var session = new GameSession(grid, 3, 5, NoColor, Obstacles(O(ObstacleKind.Pinata, 1)), random);

            TurnResult result = session.Tap(1, 1);

            Assert.False(result.HasBlasted);
            Assert.Equal(5, session.MovesLeft);
            Assert.Equal(1, session.Remaining(ObstacleKind.Pinata));
            Assert.Equal(0, random.Calls);
        }

        [Fact]
        public void Tap_LastMoveCompletesOnlyObstacleGoal_Wins()
        {
            Grid grid = ObstacleGrid.Parse("R P B B", "R R B G");
            var session = new GameSession(grid, 3, 1, NoColor, Obstacles(O(ObstacleKind.Pinata, 1)), CountingRandom.Constant(1));

            session.Tap(0, 0);

            Assert.Equal(GameStatus.Won, session.Status);
        }

        [Fact]
        public void Tap_ColorDoneObstacleLeftOnLastMove_Loses()
        {
            Grid grid = ObstacleGrid.Parse("R P B B", "R R B G");
            var session = new GameSession(grid, 3, 1, Colors(C(TileColor.Red, 3)),
                Obstacles(O(ObstacleKind.Pinata, 2)), CountingRandom.Constant(1));

            session.Tap(0, 0);

            Assert.Equal(0, session.Remaining(TileColor.Red));
            Assert.Equal(1, session.Remaining(ObstacleKind.Pinata));
            Assert.Equal(GameStatus.Lost, session.Status);
        }
    }
}
