using System.Collections.Generic;
using Xunit;

namespace BlastGame.Core.Tests
{
    public class GameSessionSpecialTests
    {
        private static readonly string[] Example1 = { "BBGGG", "RRRRR", "GBGBG" };
        private static readonly string[] Example1After = { "RRGRR", "BBVGG", "GBGBG" };
        private static readonly string[] Example3 = { "RGBGR", "GXHBG", "BRGRB" };

        private static GameSession Session(string[] rows, int moves, IRandomSource random, params ColorGoal[] goals)
        {
            return new GameSession(SpecialGrid.Parse(rows), 3, moves, new List<ColorGoal>(goals), random);
        }

        private static ColorGoal G(TileColor color, int count)
        {
            return new ColorGoal(color, count);
        }

        [Fact]
        public void Tap_Rocket_SpendsMoveAndCountsNoItem()
        {
            GameSession session = Session(Example1After, 3, CountingRandom.Constant(0), G(TileColor.Red, 10));

            TurnResult result = session.Tap(2, 1);

            Assert.True(result.HasBlasted);
            Assert.Single(result.Effects);
            Assert.Equal(2, session.MovesLeft);
            Assert.Equal(10, session.Remaining(TileColor.Red));
            Assert.Equal(GameStatus.Playing, session.Status);
        }

        [Fact]
        public void Tap_Example3Chain_EachColorCountedSeparatelyItemsNotCounted()
        {
            GameSession session = Session(Example3, 5, CountingRandom.Constant(0),
                G(TileColor.Red, 5), G(TileColor.Green, 5), G(TileColor.Blue, 5));

            session.Tap(1, 1);

            Assert.Equal(3, session.Remaining(TileColor.Red));
            Assert.Equal(1, session.Remaining(TileColor.Green));
            Assert.Equal(2, session.Remaining(TileColor.Blue));
            Assert.Equal(4, session.MovesLeft);
            Assert.Equal(GameStatus.Playing, session.Status);
        }

        [Fact]
        public void Tap_Example1Merge_TappedCellCountedFiveRed()
        {
            GameSession session = Session(Example1, 5, new CountingRandom(1, 0, 0, 0, 0), G(TileColor.Red, 10));

            TurnResult result = session.Tap(2, 1);

            Assert.True(Cell.OfSpecial(SpecialKind.RocketVertical) == result.Effects[0].Created);
            Assert.Equal(5, session.Remaining(TileColor.Red));
            Assert.Equal(4, session.MovesLeft);
        }

        [Fact]
        public void Tap_Example4Orb_OrbItselfNotCounted()
        {
            GameSession session = Session(new[] { "RrBR", "BRRG" }, 5, CountingRandom.Constant(1), G(TileColor.Red, 10));

            session.Tap(1, 1);

            Assert.Equal(6, session.Remaining(TileColor.Red));
        }

        [Fact]
        public void Tap_ChainClearsMoreThanGoal_RemainingStopsAtZero()
        {
            GameSession session = Session(Example3, 5, CountingRandom.Constant(0),
                G(TileColor.Green, 2), G(TileColor.Red, 10));

            session.Tap(1, 1);

            Assert.Equal(0, session.Remaining(TileColor.Green));
            Assert.Equal(8, session.Remaining(TileColor.Red));
            Assert.Equal(GameStatus.Playing, session.Status);
        }

        [Fact]
        public void Tap_ChainOnLastMove_MeetsAllGoalsAndWins()
        {
            GameSession session = Session(Example3, 1, CountingRandom.Constant(0),
                G(TileColor.Red, 2), G(TileColor.Green, 4), G(TileColor.Blue, 3));

            session.Tap(1, 1);

            Assert.Equal(GameStatus.Won, session.Status);
            Assert.Equal(0, session.MovesLeft);
        }

        [Fact]
        public void Tap_SpecialAfterGameOver_ChangesNothingSkipsSourceEffectsEmpty()
        {
            var random = CountingRandom.Constant(1);
            GameSession session = Session(new[] { "XGB", "RRG" }, 5, random, G(TileColor.Red, 2));
            session.Tap(0, 0);   // two red out: goal met, game won; the bomb falls
            Assert.Equal(GameStatus.Won, session.Status);

            int bx = -1;
            int by = -1;
            for (int y = 0; y < session.Grid.Height && bx < 0; y++)
            {
                for (int x = 0; x < session.Grid.Width; x++)
                {
                    if (session.Grid.Get(x, y).Kind == CellKind.Special)
                    {
                        bx = x;
                        by = y;
                        break;
                    }
                }
            }

            Assert.True(bx >= 0, "The bomb should still be on the board.");
            string[] before = SpecialGrid.Dump(session.Grid);
            int calls = random.Calls;
            int moves = session.MovesLeft;

            TurnResult result = session.Tap(bx, by);

            Assert.False(result.HasBlasted);
            Assert.Empty(result.Blasted);
            Assert.Empty(result.Moves);
            Assert.Empty(result.Filled);
            Assert.NotNull(result.Effects);
            Assert.Empty(result.Effects);
            SpecialGrid.AssertRows(before, session.Grid);
            Assert.Equal(calls, random.Calls);
            Assert.Equal(moves, session.MovesLeft);
            Assert.Equal(GameStatus.Won, session.Status);
        }
    }
}
