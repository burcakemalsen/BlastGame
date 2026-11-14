using System.Collections.Generic;
using Xunit;

namespace BlastGame.Core.Tests
{
    /// <summary>
    /// Gaps found in the step 6b review: priority with repeated kinds, RocketBomb off-center strips
    /// reaching items, goal clamping through Converted, and losing on a combo.
    /// Grids top row first (y grows upward).
    /// </summary>
    public class ComboReviewFollowUpTests
    {
        private static readonly string[] Example5 = { "RBRB", "BrHB", "RBBB" };

        [Theory]
        [InlineData("BrXXB", 1, EffectKind.OrbBomb)]     // 1 Orb + 2 Bomb: not BigBomb
        [InlineData("BrgXB", 1, EffectKind.OrbOrb)]      // 2 Orb + 1 Bomb: not OrbBomb
        [InlineData("BHHrB", 1, EffectKind.OrbRocket)]   // 1 Orb + 2 Rocket: not RocketCross
        public void Tap_RepeatedKindsWithOneOrb_OrbComboWins(string row, int tapX, EffectKind kind)
        {
            Grid grid = SpecialGrid.Parse(row);

            IReadOnlyList<TurnEffect> effects = SpecialRules.Tap(grid, tapX, 0, CountingRandom.Constant(0));

            Assert.Equal(kind, effects[0].Kind);
            Assert.Equal(3, effects[0].MergedCount);
        }

        // RocketBomb at (3,3): rows 2-4, columns 2-4. A bomb in column 2 (ox-1) and a rocket in row 4 (oy+1),
        // both outside the center row/column, are queued in y then x order and run alone.
        [Fact]
        public void Tap_RocketBombOffCenterStrips_ItemsQueuedRunAlone()
        {
            Grid grid = ComboHelpers.Board(7, 7, "RGBY", "3,3,X", "4,3,H", "2,0,X", "0,4,V");

            IReadOnlyList<TurnEffect> effects = SpecialRules.Tap(grid, 3, 3, new CountingRandom());

            Assert.Equal(new[] { "RocketBomb 3,3 t=-1", "Bomb 2,0 t=0", "Rocket 0,4 t=0" }, ComboHelpers.Heads(effects));
            Assert.Equal(1, effects[1].MergedCount);
            Assert.Equal(1, effects[2].MergedCount);
            Assert.Equal("2,0X", SpecialGrid.Format(new[] { effects[1].Cleared[0] }));
            Assert.Equal("0,4V", SpecialGrid.Format(new[] { effects[2].Cleared[0] }));
            SpecialGrid.AssertClearedUnique(effects);
        }

        private static GameSession Session(int moves, params ColorGoal[] goals)
        {
            return new GameSession(SpecialGrid.Parse(Example5), 3, moves, new List<ColorGoal>(goals),
                new CountingRandom(1, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0));
        }

        // Example 5 converts 3 red cells; a red goal of 2 stops at 0.
        [Fact]
        public void SessionTap_ConvertedMoreThanGoal_RemainingStopsAtZero()
        {
            GameSession session = Session(5, new ColorGoal(TileColor.Red, 2), new ColorGoal(TileColor.Blue, 10));

            session.Tap(1, 1);

            Assert.Equal(0, session.Remaining(TileColor.Red));
            Assert.Equal(6, session.Remaining(TileColor.Blue));
            Assert.Equal(GameStatus.Playing, session.Status);
        }

        [Fact]
        public void SessionTap_ComboOnLastMoveGoalNotMet_Lost()
        {
            GameSession session = Session(1, new ColorGoal(TileColor.Red, 10));

            TurnResult result = session.Tap(1, 1);

            Assert.True(result.HasBlasted);
            Assert.Equal(7, session.Remaining(TileColor.Red));
            Assert.Equal(0, session.MovesLeft);
            Assert.Equal(GameStatus.Lost, session.Status);
        }
    }
}
