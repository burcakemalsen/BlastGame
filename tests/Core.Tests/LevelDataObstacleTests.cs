using System;
using System.Collections.Generic;
using Xunit;

namespace BlastGame.Core.Tests
{
    /// <summary>Spec 07, LevelData: placements, obstacle goals, CreateSession, reproducibility.</summary>
    public class LevelDataObstacleTests
    {
        private static readonly List<ColorGoal> NoColor = new List<ColorGoal>();
        private static readonly List<ObstacleGoal> NoObstacleGoal = new List<ObstacleGoal>();

        private static ObstaclePlacement Pl(ObstacleKind kind, int x, int y, int layers = 1) =>
            new ObstaclePlacement(kind, new CellPos(x, y), layers);

        private static List<ObstaclePlacement> Places(params ObstaclePlacement[] p) => new List<ObstaclePlacement>(p);

        private static List<ObstacleGoal> OGoals(params ObstacleGoal[] g) => new List<ObstacleGoal>(g);

        private static List<ColorGoal> RedGoal() => new List<ColorGoal> { new ColorGoal(TileColor.Red, 5) };

        private static List<ObstaclePlacement> FourKinds() => Places(
            Pl(ObstacleKind.Crate, 0, 1, 3),
            Pl(ObstacleKind.Pinata, 1, 1),
            Pl(ObstacleKind.WaterBalloon, 2, 2),
            Pl(ObstacleKind.Anchor, 3, 3));

        private static LevelData Level(List<ObstaclePlacement> obstacles, List<ObstacleGoal> goals) =>
            new LevelData(4, 4, 3, 10, NoColor, obstacles, goals);

        [Fact]
        public void Constructor_Valid_KeepsCopiesOfLists()
        {
            var obstacles = FourKinds();
            var goals = OGoals(new ObstacleGoal(ObstacleKind.Crate, 1), new ObstacleGoal(ObstacleKind.WaterBalloon, 1));

            var level = new LevelData(4, 4, 3, 10, RedGoal(), obstacles, goals);
            obstacles.Clear();
            goals.Clear();

            Assert.Equal(FourKinds(), level.Obstacles);
            Assert.Equal(new[] { new ObstacleGoal(ObstacleKind.Crate, 1), new ObstacleGoal(ObstacleKind.WaterBalloon, 1) },
                level.ObstacleGoals);
            Assert.Single(level.Goals);
        }

        [Fact]
        public void Constructor_FiveParameters_HasNoObstacles()
        {
            var level = new LevelData(4, 4, 3, 10, RedGoal());

            Assert.NotNull(level.Obstacles);
            Assert.Empty(level.Obstacles);
            Assert.NotNull(level.ObstacleGoals);
            Assert.Empty(level.ObstacleGoals);
        }

        [Fact]
        public void Constructor_NullObstacles_ThrowsArgumentNull()
        {
            var ex = Assert.Throws<ArgumentNullException>(() => Level(null, null));
            Assert.Equal("obstacles", ex.ParamName);
        }

        public static TheoryData<ObstaclePlacement[]> BadPlacements => new TheoryData<ObstaclePlacement[]>
        {
            new[] { Pl(ObstacleKind.Pinata, 4, 1) },
            new[] { Pl(ObstacleKind.Pinata, 0, 4) },
            new[] { Pl(ObstacleKind.Pinata, 1, 1), Pl(ObstacleKind.WaterBalloon, 1, 1) },
            new[] { Pl(ObstacleKind.Anchor, 2, 0) },
        };

        [Theory]
        [MemberData(nameof(BadPlacements))]
        public void Constructor_BadPlacement_ThrowsArgumentExceptionBeforeObstacleGoals(ObstaclePlacement[] placements)
        {
            var ex = Assert.Throws<ArgumentException>(() => Level(Places(placements), null));
            Assert.Equal("obstacles", ex.ParamName);
        }

        [Fact]
        public void Constructor_NullObstacleGoals_ThrowsArgumentNull()
        {
            var ex = Assert.Throws<ArgumentNullException>(() => Level(FourKinds(), null));
            Assert.Equal("obstacleGoals", ex.ParamName);
        }

        [Fact]
        public void Constructor_NoGoalsAtAll_ThrowsArgumentExceptionGoals()
        {
            var ex = Assert.Throws<ArgumentException>(() => Level(FourKinds(), NoObstacleGoal));
            Assert.Equal("goals", ex.ParamName);
        }

        public static TheoryData<ObstacleGoal[]> BadObstacleGoals => new TheoryData<ObstacleGoal[]>
        {
            new[] { new ObstacleGoal(ObstacleKind.Pinata, 1), new ObstacleGoal(ObstacleKind.Pinata, 1) },
            new[] { new ObstacleGoal(ObstacleKind.Anchor, 0) },
            new[] { new ObstacleGoal(ObstacleKind.Crate, 2) },   // one Crate (3 layers) counts as 1
        };

        [Theory]
        [MemberData(nameof(BadObstacleGoals))]
        public void Constructor_BadObstacleGoals_ThrowsArgumentException(ObstacleGoal[] goals)
        {
            var ex = Assert.Throws<ArgumentException>(() => Level(FourKinds(), OGoals(goals)));
            Assert.Equal("obstacleGoals", ex.ParamName);
        }

        [Fact]
        public void Constructor_BadMovesAndNullObstacles_MovesFirst()
        {
            var ex = Assert.Throws<ArgumentOutOfRangeException>(() => new LevelData(4, 4, 3, 0, NoColor, null, null));
            Assert.Equal("moves", ex.ParamName);
        }

        [Fact]
        public void CreateSession_PlacesObstaclesOnSeededGrid()
        {
            var level = new LevelData(4, 4, 3, 10, RedGoal(), FourKinds(),
                OGoals(new ObstacleGoal(ObstacleKind.Anchor, 1)));

            GameSession session = level.CreateSession(new SeededRandom(7));

            Grid expected = GridFactory.CreateRandom(4, 4, 3, new SeededRandom(7));
            expected.Set(0, 1, Cell.OfObstacle(ObstacleKind.Crate, 3));
            expected.Set(1, 1, Cell.OfObstacle(ObstacleKind.Pinata, 1));
            expected.Set(2, 2, expected.Get(2, 2).WithBalloon(true));
            expected.Set(3, 3, Cell.OfObstacle(ObstacleKind.Anchor, 1));
            SpecialGrid.AssertSameCells(expected, session.Grid);
            Assert.Equal(new[] { new ObstacleGoal(ObstacleKind.Anchor, 1) }, session.ObstacleGoals);
            Assert.Equal(10, session.MovesLeft);
        }

        [Fact]
        public void CreateSession_SameSeedSameTaps_SameGridsAndResults()
        {
            var level = new LevelData(6, 6, 3, 30, RedGoal(), Places(
                    Pl(ObstacleKind.Crate, 2, 1, 2),
                    Pl(ObstacleKind.Pinata, 3, 2),
                    Pl(ObstacleKind.WaterBalloon, 1, 3),
                    Pl(ObstacleKind.Anchor, 4, 5)),
                OGoals(new ObstacleGoal(ObstacleKind.Anchor, 1)));
            var taps = new[] { 0, 0, 3, 1, 1, 3, 5, 5, 2, 0, 4, 4, 0, 5, 3, 3 };

            string first = Play(level.CreateSession(new SeededRandom(99)), taps);
            string second = Play(level.CreateSession(new SeededRandom(99)), taps);

            Assert.Equal(first, second);
        }

        private static string Play(GameSession session, int[] taps)
        {
            var log = new List<string>();
            for (int i = 0; i < taps.Length; i += 2)
            {
                TurnResult r = session.Tap(taps[i], taps[i + 1]);
                var hits = new List<string>();
                foreach (TurnEffect e in r.Effects)
                {
                    hits.Add(string.Join("/", ObstacleGrid.Format(e.Hits)) + "|" + ObstacleGrid.Cleared(e));
                }

                log.Add(string.Join(";", hits) + " m=" + TurnTestHelpers.Format(r.Moves) +
                        " c=" + TurnTestHelpers.Format(r.Collected) + " f=" + TurnTestHelpers.Format(r.Filled));
            }

            log.Add(string.Join("/", ObstacleGrid.Dump(session.Grid)));
            log.Add(session.Remaining(ObstacleKind.Anchor) + " " + session.MovesLeft + " " + session.Status);
            return string.Join("\n", log);
        }
    }
}
