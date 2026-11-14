using System.Collections.Generic;
using Xunit;

namespace BlastGame.Core.Tests
{
    /// <summary>
    /// Combo chain (spec 06b): items in a combo area are queued (y then x) and run alone; a chained
    /// item never combos with its neighbours. OrbOrb clears the whole board without triggering items.
    /// Grids top row first (y grows upward).
    /// </summary>
    public class ComboChainTests
    {
        // The bomb in the cross's column runs alone although a rocket touches it (no RocketBomb);
        // it reaches that rocket, which then runs alone too.
        [Fact]
        public void Tap_ItemInComboArea_RunsAloneNeighbourNotMerged()
        {
            Grid grid = SpecialGrid.Parse("RGBYP", "VXBGR", "BYGPG", "GHVBY", "PRGBY");
            var random = new CountingRandom();

            IReadOnlyList<TurnEffect> effects = SpecialRules.Tap(grid, 1, 1, random);

            ComboHelpers.AssertEffects(effects,
                "RocketCross 1,1 t=-1 c=. | 1,1H 2,1V 1,0R 0,1G 3,1B 4,1Y 1,2Y 1,4G || m=2 cv=",
                "Bomb 1,3 t=0 c=. | 1,3X 0,2B 2,2G 2,3B 0,4R 2,4B || m=1 cv=",
                "Rocket 0,3 t=1 c=. | 0,3V 0,0P || m=1 cv=");
            Assert.Equal(0, random.Calls);
            SpecialGrid.AssertClearedUnique(effects);
        }

        // Items in the cross are queued in y then x order, not by distance from the center.
        [Fact]
        public void Tap_SeveralItemsInCross_QueuedInYThenXOrder()
        {
            Grid grid = ComboHelpers.Board(7, 7, "RGBY", "3,3,H", "4,3,V", "3,0,X", "1,3,X", "3,5,X");

            IReadOnlyList<TurnEffect> effects = SpecialRules.Tap(grid, 3, 3, new CountingRandom());

            Assert.Equal(new[] { "RocketCross 3,3 t=-1", "Bomb 3,0 t=0", "Bomb 1,3 t=0", "Bomb 3,5 t=0" },
                ComboHelpers.Heads(effects));
            SpecialGrid.AssertClearedUnique(effects);
        }

        // BigBomb reaches 3 cells away (bomb at dx 3, rocket at the 7x7 corner) but not 4 (rocket at dy 4).
        [Fact]
        public void Tap_BigBombAreaEdge_ItemsAtDistanceThreeRunFourDoesNot()
        {
            Grid grid = ComboHelpers.Board(9, 9, "RGBY", "4,4,X", "5,4,X", "1,4,X", "7,7,H", "4,0,V");

            IReadOnlyList<TurnEffect> effects = SpecialRules.Tap(grid, 4, 4, new CountingRandom());

            Assert.Equal(new[] { "BigBomb 4,4 t=-1", "Bomb 1,4 t=0", "Rocket 7,7 t=0" }, ComboHelpers.Heads(effects));
            Assert.Equal(SpecialGrid.CellOf('V'), grid.Get(4, 0));
        }

        // ---------- OrbOrb ----------

        // Example 7: the whole board goes, other items (even a touching group of them) are only cleared.
        [Fact]
        public void Tap_Example7OrbOrb_WholeBoardClearedOneEffect()
        {
            Grid grid = SpecialGrid.Parse("RGHX", "grBV", "YBGb");
            var random = new CountingRandom();

            IReadOnlyList<TurnEffect> effects = SpecialRules.Tap(grid, 1, 1, random);

            ComboHelpers.AssertEffects(effects,
                "OrbOrb 1,1 t=-1 c=. | 1,1r 0,1g 0,0Y 1,0B 2,0G 3,0b 2,1B 3,1V 0,2R 1,2G 2,2H 3,2X || m=2 cv=");
            SpecialGrid.AssertRows(new[] { "....", "....", "...." }, grid);
            Assert.Equal(0, random.Calls);
        }

        [Fact]
        public void Tap_OrbOrbWithHoles_HolesSkipped()
        {
            Grid grid = SpecialGrid.Parse(".rg.", "R.GX");

            IReadOnlyList<TurnEffect> effects = SpecialRules.Tap(grid, 1, 1, new CountingRandom());

            ComboHelpers.AssertEffects(effects, "OrbOrb 1,1 t=-1 c=. | 1,1r 2,1g 0,0R 2,0G 3,0X || m=2 cv=");
            SpecialGrid.AssertRows(new[] { "....", "...." }, grid);
        }
    }
}
