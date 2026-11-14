using System;
using System.Collections.Generic;
using Xunit;

namespace BlastGame.Core.Tests
{
    /// <summary>
    /// GameSession with combos (spec 06b): a combo spends one move; colored cells in every Cleared
    /// and every Converted count toward goals, items never do. Grids top row first.
    /// </summary>
    public class GameSessionComboTests
    {
        private static readonly string[] Example5 = { "RBRB", "BrHB", "RBBB" };

        private static GameSession Session(string[] rows, int colorCount, int moves, IRandomSource random,
                                           params ColorGoal[] goals)
        {
            return new GameSession(SpecialGrid.Parse(rows), colorCount, moves, new List<ColorGoal>(goals), random);
        }

        private static ColorGoal G(TileColor color, int count)
        {
            return new ColorGoal(color, count);
        }

        private static CountingRandom Example5Source()
        {
            return new CountingRandom(1, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0);
        }

        [Fact]
        public void Tap_Example1RocketCross_SpendsOneMoveCountsColoredCells()
        {
            GameSession session = Session(new[] { "RGBYP", "GBYPR", "BYHVG", "YPRGB", "PRGBY" }, 5, 3,
                CountingRandom.Constant(0), G(TileColor.Red, 20), G(TileColor.Yellow, 10));

            TurnResult result = session.Tap(2, 2);

            Assert.True(result.HasBlasted);
            Assert.Equal(EffectKind.RocketCross, result.Effects[0].Kind);
            Assert.Equal(2, session.MovesLeft);
            Assert.Equal(19, session.Remaining(TileColor.Red));
            Assert.Equal(8, session.Remaining(TileColor.Yellow));
            Assert.Equal(GameStatus.Playing, session.Status);
        }

        // Example 5: Red 3 (all from Converted), Blue 4.
        [Fact]
        public void Tap_Example5OrbRocket_ConvertedCountedOrbNotCounted()
        {
            GameSession session = Session(Example5, 3, 5, Example5Source(), G(TileColor.Red, 5), G(TileColor.Blue, 10));

            session.Tap(1, 1);

            Assert.Equal(2, session.Remaining(TileColor.Red));
            Assert.Equal(6, session.Remaining(TileColor.Blue));
            Assert.Equal(4, session.MovesLeft);
        }

        // Example 6: Red 4 from Converted (the red Orb itself not counted), Green 3, Blue 4.
        [Fact]
        public void Tap_Example6OrbBomb_ConvertedAndChainCountedParticipantsNot()
        {
            GameSession session = Session(new[] { "GBGB", "BRXG", "RrHB", "GRBR" }, 3, 5, CountingRandom.Constant(0),
                G(TileColor.Red, 10), G(TileColor.Green, 10), G(TileColor.Blue, 10));

            session.Tap(1, 1);

            Assert.Equal(6, session.Remaining(TileColor.Red));
            Assert.Equal(7, session.Remaining(TileColor.Green));
            Assert.Equal(6, session.Remaining(TileColor.Blue));
        }

        [Fact]
        public void Tap_OrbOrb_OnlyColoredCellsCounted()
        {
            GameSession session = Session(new[] { "RGHX", "grBV", "RBGb" }, 3, 5, CountingRandom.Constant(0),
                G(TileColor.Red, 10), G(TileColor.Green, 10), G(TileColor.Blue, 10));

            session.Tap(1, 1);

            Assert.Equal(8, session.Remaining(TileColor.Red));
            Assert.Equal(8, session.Remaining(TileColor.Green));
            Assert.Equal(8, session.Remaining(TileColor.Blue));
        }

        [Fact]
        public void Tap_ComboOnLastMoveMeetsGoalsThroughConverted_Won()
        {
            GameSession session = Session(Example5, 3, 1, Example5Source(), G(TileColor.Red, 3), G(TileColor.Blue, 4));

            session.Tap(1, 1);

            Assert.Equal(GameStatus.Won, session.Status);
            Assert.Equal(0, session.MovesLeft);
        }

        [Fact]
        public void Tap_ComboInvalidDirectionDraw_ThrowsStateUnchanged()
        {
            GameSession session = Session(Example5, 3, 5, new CountingRandom(1, 2, 0, 0), G(TileColor.Red, 5));
            string[] before = SpecialGrid.Dump(session.Grid);

            Assert.Throws<InvalidOperationException>(() => session.Tap(1, 1));

            SpecialGrid.AssertRows(before, session.Grid);
            Assert.Equal(5, session.MovesLeft);
            Assert.Equal(5, session.Remaining(TileColor.Red));
            Assert.Equal(GameStatus.Playing, session.Status);
        }
    }
}
