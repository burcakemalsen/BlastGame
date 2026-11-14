using System;
using Xunit;

namespace BlastGame.Core.Tests
{
    /// <summary>
    /// SpecialRules.Activate. Grids are written top row first (y grows upward), letters as in
    /// <see cref="SpecialGrid"/>. Effect lines: "Kind x,y t=TriggeredBy c=Created | cleared in order".
    /// Activate does not apply gravity or refill, so cleared cells stay empty.
    /// </summary>
    public class SpecialRulesActivateTests
    {
        private static void AssertActivate(string[] before, int x, int y, string[] after, params string[] effects)
        {
            Grid grid = SpecialGrid.Parse(before);

            var result = SpecialRules.Activate(grid, x, y);

            SpecialGrid.AssertEffects(result, effects);
            SpecialGrid.AssertClearedUnique(result);
            SpecialGrid.AssertRows(after, grid);
        }

        // ---------- errors ----------

        [Fact]
        public void Activate_NullGrid_ThrowsArgumentNullWithName()
        {
            var ex = Assert.Throws<ArgumentNullException>(() => SpecialRules.Activate(null, -1, -1));
            Assert.Equal("grid", ex.ParamName);
        }

        [Theory]
        [InlineData(-1, 0, "x")]
        [InlineData(3, 0, "x")]
        [InlineData(0, -1, "y")]
        [InlineData(0, 2, "y")]
        [InlineData(-1, 9, "x")]
        public void Activate_Outside_ThrowsOutOfRangeXBeforeY(int x, int y, string expected)
        {
            string[] rows = { "RXG", "HBV" };
            Grid grid = SpecialGrid.Parse(rows);

            var ex = Assert.Throws<ArgumentOutOfRangeException>(() => SpecialRules.Activate(grid, x, y));
            Assert.Equal(expected, ex.ParamName);
            SpecialGrid.AssertRows(rows, grid);
        }

        [Theory]
        [InlineData(0, 1)]   // colored
        [InlineData(2, 1)]   // empty
        public void Activate_NotSpecial_ThrowsArgumentExceptionGridUntouched(int x, int y)
        {
            string[] rows = { "RX.", "HBV" };
            Grid grid = SpecialGrid.Parse(rows);

            Assert.Throws<ArgumentException>(() => SpecialRules.Activate(grid, x, y));
            SpecialGrid.AssertRows(rows, grid);
        }

        // ---------- Rocket ----------

        [Fact]
        public void Activate_HorizontalRocketMiddle_ClearsRowNearestFirstLeftBeforeRight()
        {
            AssertActivate(new[] { "GGGGG", "RBHYP", "GGGGG" }, 2, 1,
                new[] { "GGGGG", ".....", "GGGGG" },
                "Rocket 2,1 t=-1 c=. | 2,1H 1,1B 3,1Y 0,1R 4,1P");
        }

        [Fact]
        public void Activate_VerticalRocketMiddle_ClearsColumnNearestFirstDownBeforeUp()
        {
            AssertActivate(new[] { "GRG", "GBG", "GVG", "GYG", "GPG" }, 1, 2,
                new[] { "G.G", "G.G", "G.G", "G.G", "G.G" },
                "Rocket 1,2 t=-1 c=. | 1,2V 1,1Y 1,3B 1,0P 1,4R");
        }

        [Fact]
        public void Activate_HorizontalRocketNearLeftEdge_SkipsOutsideKeepsDistanceOrder()
        {
            AssertActivate(new[] { "BBBBB", "RHGYP" }, 1, 0,
                new[] { "BBBBB", "....." },
                "Rocket 1,0 t=-1 c=. | 1,0H 0,0R 2,0G 3,0Y 4,0P");
        }

        [Fact]
        public void Activate_VerticalRocketTopRightCorner_ClearsDownOnly()
        {
            AssertActivate(new[] { "GGGV", "GGGR", "GGGB", "GGGY" }, 3, 3,
                new[] { "GGG.", "GGG.", "GGG.", "GGG." },
                "Rocket 3,3 t=-1 c=. | 3,3V 3,2R 3,1B 3,0Y");
        }

        [Fact]
        public void Activate_HorizontalRocketOnWidthOneBoard_OnlyItself()
        {
            AssertActivate(new[] { "R", "H", "G" }, 0, 1,
                new[] { "R", ".", "G" },
                "Rocket 0,1 t=-1 c=. | 0,1H");
        }

        [Fact]
        public void Activate_VerticalRocketOnHeightOneBoard_OnlyItself()
        {
            AssertActivate(new[] { "RVG" }, 1, 0,
                new[] { "R.G" },
                "Rocket 1,0 t=-1 c=. | 1,0V");
        }

        [Fact]
        public void Activate_RocketOverEmptyCells_SkipsThem()
        {
            AssertActivate(new[] { "R.H.G" }, 2, 0,
                new[] { "....." },
                "Rocket 2,0 t=-1 c=. | 2,0H 0,0R 4,0G");
        }

        // ---------- Bomb ----------

        [Fact]
        public void Activate_BombMiddle_ClearsNineInRowOrder()
        {
            AssertActivate(new[] { "RRRRR", "RGBYR", "RPXOR", "RBGYR", "RRRRR" }, 2, 2,
                new[] { "RRRRR", "R...R", "R...R", "R...R", "RRRRR" },
                "Bomb 2,2 t=-1 c=. | 2,2X 1,1B 2,1G 3,1Y 1,2P 3,2O 1,3G 2,3B 3,3Y");
        }

        [Theory]
        [InlineData(0, 0, "Bomb 0,0 t=-1 c=. | 0,0X 1,0G 0,1G 1,1G")]
        [InlineData(3, 0, "Bomb 3,0 t=-1 c=. | 3,0X 2,0G 2,1G 3,1G")]
        [InlineData(0, 3, "Bomb 0,3 t=-1 c=. | 0,3X 0,2G 1,2G 1,3G")]
        [InlineData(3, 3, "Bomb 3,3 t=-1 c=. | 3,3X 2,2G 3,2G 2,3G")]
        public void Activate_BombCorner_ClearsFour(int x, int y, string expected)
        {
            Grid grid = TestGrid.Filled(4, 4, TileColor.Green);
            grid.Set(x, y, Cell.OfSpecial(SpecialKind.Bomb));

            var result = SpecialRules.Activate(grid, x, y);

            SpecialGrid.AssertEffects(result, expected);
            int left = 0;
            for (int cy = 0; cy < 4; cy++)
            {
                for (int cx = 0; cx < 4; cx++)
                {
                    if (!grid.Get(cx, cy).IsEmpty)
                    {
                        left++;
                    }
                }
            }

            Assert.Equal(12, left);
        }

        [Fact]
        public void Activate_BombOnBottomEdge_ClearsSix()
        {
            AssertActivate(new[] { "GGGGG", "GGGGG", "GGXGG" }, 2, 0,
                new[] { "GGGGG", "G...G", "G...G" },
                "Bomb 2,0 t=-1 c=. | 2,0X 1,0G 3,0G 1,1G 2,1G 3,1G");
        }

        [Fact]
        public void Activate_BombOnOneByOneBoard_OnlyItself()
        {
            AssertActivate(new[] { "X" }, 0, 0, new[] { "." }, "Bomb 0,0 t=-1 c=. | 0,0X");
        }

        // ---------- Orb ----------

        [Fact]
        public void Activate_Example4Orb_ClearsItselfThenItsColorInRowOrder()
        {
            AssertActivate(new[] { "RrBR", "BRRG" }, 1, 1,
                new[] { "..B.", "B..G" },
                "Orb 1,1 t=-1 c=. | 1,1r 1,0R 2,0R 0,1R 3,1R");
        }

        [Fact]
        public void Activate_OrbWithoutItsColor_OnlyItself()
        {
            AssertActivate(new[] { "BrG", "GBY" }, 1, 1,
                new[] { "B.G", "GBY" },
                "Orb 1,1 t=-1 c=. | 1,1r");
        }

        [Fact]
        public void Activate_OrbAmongOtherSpecials_IgnoresOrbsOfAnyColorAndStartsNoChain()
        {
            AssertActivate(new[] { "RrXr", "bRHR" }, 1, 1,
                new[] { "..Xr", "b.H." },
                "Orb 1,1 t=-1 c=. | 1,1r 1,0R 3,0R 0,1R");
        }

        // ---------- chains ----------

        [Fact]
        public void Activate_Example3BombThenRocket_TwoEffectsWithTriggeredBy()
        {
            AssertActivate(new[] { "RGBGR", "GXHBG", "BRGRB" }, 1, 1,
                new[] { "...GR", ".....", "...RB" },
                "Bomb 1,1 t=-1 c=. | 1,1X 0,0B 1,0R 2,0G 0,1G 0,2R 1,2G 2,2B",
                "Rocket 2,1 t=0 c=. | 2,1H 3,1B 4,1G");
        }

        // Example 5: a horizontal rocket meets a blue orb; the orb runs after the rocket
        // and skips the blue cell the rocket already cleared.
        [Fact]
        public void Activate_Example5RocketThenOrb_OrbClearsRemainingBlues()
        {
            AssertActivate(new[] { "BGBGR", "RHGbB", "BRBGB" }, 1, 1,
                new[] { ".G.GR", ".....", ".R.G." },
                "Rocket 1,1 t=-1 c=. | 1,1H 0,1R 2,1G 4,1B",
                "Orb 3,1 t=0 c=. | 3,1b 0,0B 2,0B 4,0B 0,2B 2,2B");
        }

        // Bomb finds V then H (scan order), V finds a second bomb: queue order and TriggeredBy 1.
        [Fact]
        public void Activate_ChainOfThreeLevels_FifoOrderAndTriggeredByIndices()
        {
            AssertActivate(new[] { "RGBXY", "GHBGR", "BRXGY", "YGBVG", "RBGPR" }, 2, 2,
                new[] { "RG...", ".....", "B...Y", "Y...G", "RBG.R" },
                "Bomb 2,2 t=-1 c=. | 2,2X 1,1G 2,1B 1,2R 3,2G 2,3B 3,3G",
                "Rocket 3,1 t=0 c=. | 3,1V 3,0P",
                "Rocket 1,3 t=0 c=. | 1,3H 0,3G 4,3R",
                "Bomb 3,4 t=1 c=. | 3,4X 2,4B 4,4Y");
        }

        // H at (2,2) is in the bomb's area and in V's column: it runs once, triggered by the bomb.
        [Fact]
        public void Activate_ItemInTwoAreas_RunsOnceWithFirstFinder()
        {
            AssertActivate(new[] { "GGHB", "RXVY", "BRGP" }, 1, 1,
                new[] { "....", "...Y", "...P" },
                "Bomb 1,1 t=-1 c=. | 1,1X 0,0B 1,0R 2,0G 0,1R 0,2G 1,2G",
                "Rocket 2,1 t=0 c=. | 2,1V",
                "Rocket 2,2 t=0 c=. | 2,2H 3,2B");
        }

        [Fact]
        public void Activate_TwoRocketsSeeingEachOther_EachRunsOnceNoLoop()
        {
            AssertActivate(new[] { "GGGGG", "HRHGB" }, 0, 0,
                new[] { "GGGGG", "....." },
                "Rocket 0,0 t=-1 c=. | 0,0H 1,0R 3,0G 4,0B",
                "Rocket 2,0 t=0 c=. | 2,0H");
        }

        [Fact]
        public void Activate_AdjacentSpecialOutsideArea_NotTriggeredNoCombo()
        {
            AssertActivate(new[] { "GVG", "RHB", "GGG" }, 1, 1,
                new[] { "GVG", "...", "GGG" },
                "Rocket 1,1 t=-1 c=. | 1,1H 0,1R 2,1B");
        }
    }
}
