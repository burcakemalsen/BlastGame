using System;
using System.Collections.Generic;
using Xunit;
using static BlastGame.Core.Tests.TurnTestHelpers;

namespace BlastGame.Core.Tests
{
    /// <summary>
    /// SpecialRules.Tap (spec 06b): error order, and a lone item behaving exactly like Activate.
    /// Grids top row first (y grows upward).
    /// </summary>
    public class SpecialRulesTapTests
    {
        private static readonly string[] Pair = { "RGB", "HVB" };

        // ---------- errors ----------

        [Fact]
        public void Tap_NullGrid_ThrowsArgumentNullGrid()
        {
            var ex = Assert.Throws<ArgumentNullException>(() => SpecialRules.Tap(null, 0, 0, new CountingRandom()));
            Assert.Equal("grid", ex.ParamName);
        }

        [Fact]
        public void Tap_NullGridAndNullRandom_GridReportedFirst()
        {
            var ex = Assert.Throws<ArgumentNullException>(() => SpecialRules.Tap(null, 0, 0, null));
            Assert.Equal("grid", ex.ParamName);
        }

        [Fact]
        public void Tap_NullRandomOnCombo_ThrowsArgumentNullRandomGridUnchanged()
        {
            Grid grid = SpecialGrid.Parse(Pair);
            Grid twin = Clone(grid);

            var ex = Assert.Throws<ArgumentNullException>(() => SpecialRules.Tap(grid, 0, 0, null));
            Assert.Equal("random", ex.ParamName);
            SpecialGrid.AssertSameCells(twin, grid);
        }

        [Fact]
        public void Tap_NullRandomAndOutside_RandomReportedFirst()
        {
            Grid grid = SpecialGrid.Parse(Pair);

            var ex = Assert.Throws<ArgumentNullException>(() => SpecialRules.Tap(grid, -1, 0, null));
            Assert.Equal("random", ex.ParamName);
        }

        [Theory]
        [InlineData(-1, 0, "x")]
        [InlineData(3, 0, "x")]
        [InlineData(0, -1, "y")]
        [InlineData(0, 2, "y")]
        [InlineData(-1, -1, "x")]
        public void Tap_Outside_ThrowsOutOfRangeWithNameGridAndSourceUntouched(int x, int y, string name)
        {
            Grid grid = SpecialGrid.Parse(Pair);
            Grid twin = Clone(grid);
            var random = CountingRandom.Constant(0);

            var ex = Assert.Throws<ArgumentOutOfRangeException>(() => SpecialRules.Tap(grid, x, y, random));
            Assert.Equal(name, ex.ParamName);
            SpecialGrid.AssertSameCells(twin, grid);
            Assert.Equal(0, random.Calls);
        }

        [Theory]
        [InlineData(0, 1)]   // colored
        [InlineData(1, 1)]   // empty
        public void Tap_NotSpecial_ThrowsArgumentExceptionGridAndSourceUntouched(int x, int y)
        {
            Grid grid = SpecialGrid.Parse("R.B", "HVB");
            Grid twin = Clone(grid);
            var random = CountingRandom.Constant(0);

            Assert.Throws<ArgumentException>(() => SpecialRules.Tap(grid, x, y, random));
            SpecialGrid.AssertSameCells(twin, grid);
            Assert.Equal(0, random.Calls);
        }

        // ---------- one participant: exactly Activate ----------

        // Example 2: the bomb touches the rocket only diagonally, and is not in row 2.
        [Fact]
        public void Tap_Example2DiagonalItem_SingleRocketNoDraw()
        {
            Grid grid = SpecialGrid.Parse("RGBYP", "GBYXR", "BYHGP", "YPRGB", "PRGBY");
            var random = new CountingRandom();

            IReadOnlyList<TurnEffect> effects = SpecialRules.Tap(grid, 2, 2, random);

            ComboHelpers.AssertEffects(effects, "Rocket 2,2 t=-1 c=. | 2,2H 1,2Y 3,2G 0,2B 4,2P || m=1 cv=");
            Assert.Equal(SpecialGrid.CellOf('X'), grid.Get(3, 3));
            Assert.Equal(0, random.Calls);
        }

        // Example 8: a lone rocket; the bomb it reaches runs alone although a rocket touches it.
        [Fact]
        public void Tap_Example8ChainedItemNextToItem_NoComboSingleChain()
        {
            Grid grid = SpecialGrid.Parse("RGBYP", "GBYVR", "HRGXB");
            var random = new CountingRandom();

            IReadOnlyList<TurnEffect> effects = SpecialRules.Tap(grid, 0, 0, random);

            ComboHelpers.AssertEffects(effects,
                "Rocket 0,0 t=-1 c=. | 0,0H 1,0R 2,0G 4,0B || m=1 cv=",
                "Bomb 3,0 t=0 c=. | 3,0X 2,1Y 4,1R || m=1 cv=",
                "Rocket 3,1 t=1 c=. | 3,1V 3,2Y || m=1 cv=");
            Assert.Equal(0, random.Calls);
        }

        [Theory]
        [InlineData("RGB/GXB/BRG", 1, 1)]   // lone bomb
        [InlineData("RGR/GrB/RBR", 1, 1)]   // lone orb
        [InlineData("RXBG/GRBY/BVGR/GRBY", 1, 1)] // lone rocket, a bomb two cells up its column
        public void Tap_LoneItem_SameEffectsAndGridAsActivate(string rows, int x, int y)
        {
            Grid tapped = SpecialGrid.Parse(rows.Split('/'));
            Grid activated = SpecialGrid.Parse(rows.Split('/'));
            var random = new CountingRandom();

            IReadOnlyList<TurnEffect> viaTap = SpecialRules.Tap(tapped, x, y, random);
            IReadOnlyList<TurnEffect> viaActivate = SpecialRules.Activate(activated, x, y);

            Assert.Equal(ComboHelpers.Format(viaActivate), ComboHelpers.Format(viaTap));
            SpecialGrid.AssertSameCells(activated, tapped);
            Assert.Equal(0, random.Calls);
        }

        // Activate keeps Step 6 behaviour: no combo even with a neighbouring item.
        [Fact]
        public void Activate_AdjacentItems_NoComboStep6Chain()
        {
            Grid grid = SpecialGrid.Parse("RGBYP", "GBYPR", "BYHVG", "YPRGB", "PRGBY");

            IReadOnlyList<TurnEffect> effects = SpecialRules.Activate(grid, 2, 2);

            ComboHelpers.AssertEffects(effects,
                "Rocket 2,2 t=-1 c=. | 2,2H 1,2Y 0,2B 4,2G || m=1 cv=",
                "Rocket 3,2 t=0 c=. | 3,2V 3,1G 3,3P 3,0B 3,4Y || m=1 cv=");
        }
    }
}
