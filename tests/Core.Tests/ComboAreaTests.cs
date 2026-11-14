using System;
using System.Collections.Generic;
using Xunit;
using static BlastGame.Core.Tests.ComboHelpers;

namespace BlastGame.Core.Tests
{
    /// <summary>
    /// Area combos (spec 06b): RocketCross, RocketBomb, BigBomb. Cleared = participants, then the
    /// area's other cells y then x, empty cells skipped, clipped at the board edge. No draws.
    /// Grids top row first (y grows upward).
    /// </summary>
    public class ComboAreaTests
    {
        private static void AssertAreaCombo(Grid grid, int tx, int ty, EffectKind kind, CellPos[] participants,
                                            Func<int, int, bool> inArea, int expectedCount)
        {
            Grid before = TurnTestHelpers.Clone(grid);
            string cleared = AreaCleared(before, participants, inArea);
            var random = new CountingRandom();

            IReadOnlyList<TurnEffect> effects = SpecialRules.Tap(grid, tx, ty, random);

            AssertEffects(effects,
                kind + " " + tx + "," + ty + " t=-1 c=. | " + cleared + " || m=" + participants.Length + " cv=");
            Assert.Equal(expectedCount, effects[0].Cleared.Count);
            AssertOnlyClearedEmptied(before, grid, effects);
            Assert.Equal(0, random.Calls);
        }

        // ---------- RocketCross ----------

        [Fact]
        public void Tap_RocketCrossTopEdge_RowAndColumnClipped()
        {
            Grid grid = Board(5, 5, "RGBY", "2,4,H", "1,4,V");

            AssertAreaCombo(grid, 2, 4, EffectKind.RocketCross, new[] { P(2, 4), P(1, 4) },
                (x, y) => y == 4 || x == 2, 9);
        }

        [Fact]
        public void Tap_RocketCrossWithEmptyCells_EmptySkipped()
        {
            Grid grid = SpecialGrid.Parse("R.B", "HV.", ".GY");

            IReadOnlyList<TurnEffect> effects = SpecialRules.Tap(grid, 0, 1, new CountingRandom());

            AssertEffects(effects, "RocketCross 0,1 t=-1 c=. | 0,1H 1,1V 0,2R || m=2 cv=");
            SpecialGrid.AssertRows(new[] { "..B", "...", ".GY" }, grid);
        }

        // ---------- RocketBomb ----------

        // Example 3: rows 1-3 and columns 1-3; the rocket at (4,4) is outside and stays.
        [Fact]
        public void Tap_Example3RocketBomb_ThreeRowsThreeColumnsExactOrder()
        {
            Grid grid = SpecialGrid.Parse("RGBYV", "GBYPR", "BYXGP", "YPHRB", "PRGBY");
            var random = new CountingRandom();

            IReadOnlyList<TurnEffect> effects = SpecialRules.Tap(grid, 2, 2, random);

            AssertEffects(effects,
                "RocketBomb 2,2 t=-1 c=. | 2,2X 2,1H 1,0R 2,0G 3,0B 0,1Y 1,1P 3,1R 4,1B 0,2B 1,2Y 3,2G 4,2P " +
                "0,3G 1,3B 2,3Y 3,3P 4,3R 1,4G 2,4B 3,4Y || m=2 cv=");
            SpecialGrid.AssertRows(new[] { "R...V", ".....", ".....", ".....", "P...Y" }, grid);
            Assert.Equal(0, random.Calls);
        }

        [Fact]
        public void Tap_RocketBombCorner_MissingRowAndColumnClipped()
        {
            Grid grid = Board(5, 5, "RGBY", "0,0,X", "1,0,H");

            AssertAreaCombo(grid, 0, 0, EffectKind.RocketBomb, new[] { P(0, 0), P(1, 0) },
                (x, y) => y <= 1 || x <= 1, 16);
        }

        [Fact]
        public void Tap_RocketBombTopEdge_RowAboveClipped()
        {
            Grid grid = Board(5, 5, "RGBY", "2,4,X", "2,3,V");

            AssertAreaCombo(grid, 2, 4, EffectKind.RocketBomb, new[] { P(2, 4), P(2, 3) },
                (x, y) => y >= 3 || (x >= 1 && x <= 3), 19);
        }

        // ---------- BigBomb ----------

        // Example 4: 9x9, bombs at (0,0) and (1,0): x 0..3, y 0..3.
        [Fact]
        public void Tap_Example4BigBombCorner_SixteenCells()
        {
            Grid grid = Board(9, 9, "RGBY", "0,0,X", "1,0,X");

            AssertAreaCombo(grid, 0, 0, EffectKind.BigBomb, new[] { P(0, 0), P(1, 0) },
                (x, y) => x <= 3 && y <= 3, 16);
        }

        [Fact]
        public void Tap_BigBombCenter_SevenBySevenAndNotFurther()
        {
            Grid grid = Board(9, 9, "RGBY", "4,4,X", "4,5,X");

            AssertAreaCombo(grid, 4, 4, EffectKind.BigBomb, new[] { P(4, 4), P(4, 5) },
                (x, y) => Math.Abs(x - 4) <= 3 && Math.Abs(y - 4) <= 3, 49);
        }

        [Fact]
        public void Tap_BigBombNearBottomEdge_OnlyBottomClipped()
        {
            Grid grid = Board(9, 9, "RGBY", "4,1,X", "5,1,X");

            AssertAreaCombo(grid, 4, 1, EffectKind.BigBomb, new[] { P(4, 1), P(5, 1) },
                (x, y) => x >= 1 && x <= 7 && y <= 4, 35);
        }

        // Board smaller than 7x7, with two holes: the whole board, holes skipped.
        [Fact]
        public void Tap_BigBombOnSmallBoardWithHoles_WholeBoardHolesSkipped()
        {
            Grid grid = Board(4, 4, "RGBY", "1,1,X", "2,1,X", "0,3,.", "3,0,.");

            AssertAreaCombo(grid, 1, 1, EffectKind.BigBomb, new[] { P(1, 1), P(2, 1) },
                (x, y) => true, 14);
        }
    }
}
