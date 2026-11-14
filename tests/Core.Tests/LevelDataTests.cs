using System;
using System.Collections.Generic;
using Xunit;

namespace BlastGame.Core.Tests
{
    public class LevelDataTests
    {
        private static List<ColorGoal> RedGoal()
        {
            return new List<ColorGoal> { new ColorGoal(TileColor.Red, 5) };
        }

        [Fact]
        public void Constructor_ValidValues_ExposesThem()
        {
            var level = new LevelData(7, 5, 4, 12, new List<ColorGoal>
            {
                new ColorGoal(TileColor.Blue, 3),
                new ColorGoal(TileColor.Red, 4),
            });

            Assert.Equal((7, 5, 4, 12), (level.Width, level.Height, level.ColorCount, level.Moves));
            Assert.Equal(new[] { new ColorGoal(TileColor.Blue, 3), new ColorGoal(TileColor.Red, 4) }, level.Goals);
        }

        [Theory]
        [InlineData(0, 9, 4, 20, "width")]
        [InlineData(17, 9, 4, 20, "width")]
        [InlineData(9, 0, 4, 20, "height")]
        [InlineData(9, 17, 4, 20, "height")]
        [InlineData(9, 9, 0, 20, "colorCount")]
        [InlineData(9, 9, 7, 20, "colorCount")]
        [InlineData(9, 9, 4, 0, "moves")]
        [InlineData(17, 9, 4, 0, "width")] // first bad parameter wins
        public void Constructor_NumberOutOfRange_ThrowsArgumentOutOfRangeWithParamName(
            int width, int height, int colorCount, int moves, string paramName)
        {
            var ex = Assert.Throws<ArgumentOutOfRangeException>(
                () => new LevelData(width, height, colorCount, moves, RedGoal()));
            Assert.Equal(paramName, ex.ParamName);
        }

        [Theory]
        [InlineData(1, 1, 1, 1)]
        [InlineData(16, 16, 6, 1)]
        public void Constructor_BoundaryValues_AreAccepted(int width, int height, int colorCount, int moves)
        {
            var level = new LevelData(width, height, colorCount, moves, RedGoal());

            Assert.Equal((width, height, colorCount, moves), (level.Width, level.Height, level.ColorCount, level.Moves));
        }

        [Fact]
        public void Constructor_NullGoals_ThrowsArgumentNullException()
        {
            var ex = Assert.Throws<ArgumentNullException>(() => new LevelData(9, 9, 4, 20, null));
            Assert.Equal("goals", ex.ParamName);
        }

        public static IEnumerable<object[]> BadGoalLists()
        {
            yield return new object[] { new List<ColorGoal>() };
            yield return new object[] { new List<ColorGoal> { new ColorGoal(TileColor.Blue, 5) } }; // colorCount 2
            yield return new object[] { new List<ColorGoal> { new ColorGoal(TileColor.Red, 5), new ColorGoal(TileColor.Red, 3) } };
            yield return new object[] { new List<ColorGoal> { default(ColorGoal) } }; // Count 0; the ColorGoal constructor would reject it
        }

        [Theory]
        [MemberData(nameof(BadGoalLists))]
        public void Constructor_InvalidGoals_ThrowsArgumentExceptionWithGoalsParamName(IReadOnlyList<ColorGoal> goals)
        {
            var ex = Assert.Throws<ArgumentException>(() => new LevelData(9, 9, 2, 20, goals));
            Assert.Equal("goals", ex.ParamName);
        }

        [Fact]
        public void Constructor_GoalColorAtLastIndex_IsAccepted()
        {
            var level = new LevelData(9, 9, 3, 20, new List<ColorGoal> { new ColorGoal(TileColor.Blue, 5) });

            Assert.Single(level.Goals);
        }

        [Fact]
        public void Goals_SourceListChangedLater_LevelIsUnaffected()
        {
            List<ColorGoal> source = RedGoal();
            var level = new LevelData(9, 9, 4, 20, source);

            source.Add(new ColorGoal(TileColor.Green, 9));
            source[0] = new ColorGoal(TileColor.Red, 99);

            Assert.Equal(new[] { new ColorGoal(TileColor.Red, 5) }, level.Goals);
        }

        // ---- CreateSession ----

        [Fact]
        public void CreateSession_UsesLevelValues()
        {
            var goals = new List<ColorGoal> { new ColorGoal(TileColor.Red, 5), new ColorGoal(TileColor.Green, 7) };
            var level = new LevelData(16, 11, 2, 13, goals);

            GameSession session = level.CreateSession(new SeededRandom(1));

            Assert.Equal((16, 11), (session.Grid.Width, session.Grid.Height));
            Assert.Equal(2, session.ColorCount);
            Assert.Equal(13, session.MovesLeft);
            Assert.Equal(goals, session.Goals);
            for (int y = 0; y < 11; y++)
            {
                for (int x = 0; x < 16; x++)
                {
                    Cell cell = session.Grid.Get(x, y);
                    Assert.False(cell.IsEmpty);
                    Assert.InRange((int)cell.Color, 0, 1);
                }
            }
        }

        [Fact]
        public void CreateSession_SameSeedTwice_ProducesIdenticalGrids()
        {
            var level = new LevelData(9, 9, 4, 20, RedGoal());

            GameSession a = level.CreateSession(new SeededRandom(42));
            GameSession b = level.CreateSession(new SeededRandom(42));

            Assert.Equal(TestGrid.Dump(a.Grid), TestGrid.Dump(b.Grid));
        }

        [Fact]
        public void CreateSession_NullRandom_ThrowsArgumentNullException()
        {
            var level = new LevelData(9, 9, 4, 20, RedGoal());

            Assert.Throws<ArgumentNullException>(() => level.CreateSession(null));
        }
    }
}
