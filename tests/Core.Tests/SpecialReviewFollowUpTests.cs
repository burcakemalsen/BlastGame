using System;
using System.Collections.Generic;
using Xunit;
using static BlastGame.Core.Tests.TurnTestHelpers;

namespace BlastGame.Core.Tests
{
    /// <summary>
    /// Tests added after the step 6 review: rocket reach on non-square boards, rocket queue order,
    /// orbs inside chains, session edge cases, orb hash, specials that only fall.
    /// Grids top row first (y grows upward); letters as in <see cref="SpecialGrid"/>.
    /// </summary>
    public class SpecialReviewFollowUpTests
    {
        private static void AssertActivate(string[] before, int x, int y, string[] after, params string[] effects)
        {
            Grid grid = SpecialGrid.Parse(before);

            var result = SpecialRules.Activate(grid, x, y);

            SpecialGrid.AssertEffects(result, effects);
            SpecialGrid.AssertClearedUnique(result);
            SpecialGrid.AssertRows(after, grid);
        }

        // ---------- 1. rocket reach on non-square boards ----------

        [Fact]
        public void Activate_VerticalRocketOnTallBoard_ReachesTopCell()
        {
            AssertActivate(new[] { "GRG", "GBG", "GYG", "GPG", "GVG", "GOG" }, 1, 1,
                new[] { "G.G", "G.G", "G.G", "G.G", "G.G", "G.G" },
                "Rocket 1,1 t=-1 c=. | 1,1V 1,0O 1,2P 1,3Y 1,4B 1,5R");
        }

        [Fact]
        public void Activate_VerticalRocketOnOneByFour_ClearsWholeColumn()
        {
            AssertActivate(new[] { "R", "B", "Y", "V" }, 0, 0,
                new[] { ".", ".", ".", "." },
                "Rocket 0,0 t=-1 c=. | 0,0V 0,1Y 0,2B 0,3R");
        }

        [Fact]
        public void Activate_HorizontalRocketOnWideBoard_ReachesRightmostCell()
        {
            AssertActivate(new[] { "GGGGGG", "RHBYPO", "GGGGGG" }, 1, 1,
                new[] { "GGGGGG", "......", "GGGGGG" },
                "Rocket 1,1 t=-1 c=. | 1,1H 0,1R 2,1B 3,1Y 4,1P 5,1O");
        }

        [Fact]
        public void Activate_HorizontalRocketOnFourByOne_ClearsWholeRow()
        {
            AssertActivate(new[] { "HRBY" }, 0, 0,
                new[] { "...." },
                "Rocket 0,0 t=-1 c=. | 0,0H 1,0R 2,0B 3,0Y");
        }

        // ---------- 2. rocket queue order ----------

        // Orbs of colors absent from the board only clear themselves, so the effect order
        // shows the order the rocket queued them: nearest first, left before right.
        [Fact]
        public void Activate_HorizontalRocketFindsSeveralItems_QueuedInScanOrder()
        {
            AssertActivate(new[] { "GGGGGGG", "byHpRoG" }, 2, 0,
                new[] { "GGGGGGG", "......." },
                "Rocket 2,0 t=-1 c=. | 2,0H 4,0R 6,0G",
                "Orb 1,0 t=0 c=. | 1,0y",
                "Orb 3,0 t=0 c=. | 3,0p",
                "Orb 0,0 t=0 c=. | 0,0b",
                "Orb 5,0 t=0 c=. | 5,0o");
        }

        [Fact]
        public void Activate_VerticalRocketFindsSeveralItems_DownBeforeUpAtEqualDistance()
        {
            AssertActivate(new[] { "R", "p", "V", "y", "b" }, 0, 2,
                new[] { ".", ".", ".", ".", "." },
                "Rocket 0,2 t=-1 c=. | 0,2V 0,4R",
                "Orb 0,1 t=0 c=. | 0,1y",
                "Orb 0,3 t=0 c=. | 0,3p",
                "Orb 0,0 t=0 c=. | 0,0b");
        }

        // ---------- 3. orbs inside chains ----------

        // The bomb queues the red orb before the rocket; the orb clears (3,1) R first,
        // so the rocket skips it.
        [Fact]
        public void Activate_OrbBeforeRocketInQueue_RocketSkipsCellsOrbCleared()
        {
            AssertActivate(new[] { "GGGGR", "GXHRB", "rGGGG" }, 1, 1,
                new[] { "...G.", ".....", "...GG" },
                "Bomb 1,1 t=-1 c=. | 1,1X 1,0G 2,0G 0,1G 0,2G 1,2G 2,2G",
                "Orb 0,0 t=0 c=. | 0,0r 3,1R 4,2R",
                "Rocket 2,1 t=0 c=. | 2,1H 4,1B");
        }

        [Fact]
        public void Activate_TriggeredOrbAfterItsColorWasCleared_ClearsOnlyItself()
        {
            AssertActivate(new[] { "GGGG", "RHrR" }, 1, 0,
                new[] { "GGGG", "...." },
                "Rocket 1,0 t=-1 c=. | 1,0H 0,0R 3,0R",
                "Orb 2,0 t=0 c=. | 2,0r");
        }

        // ---------- 4. GameSession ----------

        [Fact]
        public void SessionTap_SpecialOnLastMoveGoalNotMet_Lost()
        {
            var session = new GameSession(SpecialGrid.Parse("RRGRR", "BBVGG", "GBGBG"), 3, 1,
                new List<ColorGoal> { new ColorGoal(TileColor.Red, 10) }, CountingRandom.Constant(0));

            TurnResult result = session.Tap(2, 1);

            Assert.True(result.HasBlasted);
            Assert.Equal(EffectKind.Rocket, result.Effects[0].Kind);
            Assert.Equal(0, session.MovesLeft);
            Assert.Equal(GameStatus.Lost, session.Status);
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(2)]
        public void SessionTap_RocketMergeInvalidDirection_ThrowsAndStateUnchanged(int draw)
        {
            Grid grid = SpecialGrid.Parse("BBGGG", "RRRRR", "GBGBG");
            Grid twin = Clone(grid);
            var session = new GameSession(grid, 3, 5,
                new List<ColorGoal> { new ColorGoal(TileColor.Red, 10) }, new CountingRandom(draw, 0, 0, 0, 0));

            Assert.Throws<InvalidOperationException>(() => session.Tap(2, 1));
            SpecialGrid.AssertSameCells(twin, session.Grid);
            Assert.Equal(5, session.MovesLeft);
            Assert.Equal(10, session.Remaining(TileColor.Red));
            Assert.Equal(GameStatus.Playing, session.Status);
        }

        // ---------- 5. Cell: orbs of different colors ----------

        [Fact]
        public void Orb_DifferentColors_NotEqualAsObjectAndHashesNotAllSame()
        {
            Cell red = Cell.OfOrb(TileColor.Red);
            Cell blue = Cell.OfOrb(TileColor.Blue);

            Assert.True(red != blue);
            Assert.False(red.Equals((object)blue));

            var hashes = new HashSet<int>();
            for (int c = 0; c < 6; c++)
            {
                hashes.Add(Cell.OfOrb((TileColor)c).GetHashCode());
            }

            Assert.True(hashes.Count > 1, "All six orb colors share one hash code.");
        }

        // ---------- 6. specials outside the area only fall ----------

        [Fact]
        public void Tap_RocketWithBombAboveItsRow_BombFallsNotTriggered()
        {
            // Spec 06b: a bomb right above the rocket would now join it (combo), so it sits diagonally.
            Grid grid = SpecialGrid.Parse("XGG", "BHR");

            TurnResult result = TurnRules.Tap(grid, 1, 0, 3, CountingRandom.Constant(2));

            SpecialGrid.AssertEffects(result.Effects, "Rocket 1,0 t=-1 c=. | 1,0H 0,0B 2,0R");
            AssertMoves(new[] { 0, 1, 0, 0, 1, 1, 1, 0, 2, 1, 2, 0 }, result.Moves);
            TestGrid.AssertGroup(TestGrid.Pts(0, 1, 1, 1, 2, 1), result.Filled);
            SpecialGrid.AssertRows(new[] { "BBB", "XGG" }, grid);
        }

        [Fact]
        public void Tap_GroupUnderBomb_BombFallsNotTriggered()
        {
            Grid grid = SpecialGrid.Parse("XGB", "RRG");

            TurnResult result = TurnRules.Tap(grid, 0, 0, 3, CountingRandom.Constant(1));

            SpecialGrid.AssertEffects(result.Effects, "Group 0,0 t=-1 c=. | 0,0R 1,0R");
            AssertMoves(new[] { 0, 1, 0, 0, 1, 1, 1, 0 }, result.Moves);
            TestGrid.AssertGroup(TestGrid.Pts(0, 1, 1, 1), result.Filled);
            SpecialGrid.AssertRows(new[] { "GGB", "XGG" }, grid);
        }
    }
}
