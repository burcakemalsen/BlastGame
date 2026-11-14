using System.Collections.Generic;
using Xunit;

namespace BlastGame.Core.Tests
{
    /// <summary>
    /// Who joins a combo (spec 06b, "Komboya girenler"): side-connected special cells only,
    /// tapped first then y/x order, MergedCount = participant count, all participants consumed.
    /// Grids top row first (y grows upward).
    /// </summary>
    public class ComboParticipantsTests
    {
        private static readonly string[] Example1 = { "RGBYP", "GBYPR", "BYHVG", "YPRGB", "PRGBY" };

        [Fact]
        public void Tap_Example1RocketPair_RocketCrossExactClearedNoDrawNoGravity()
        {
            Grid grid = SpecialGrid.Parse(Example1);
            var random = new CountingRandom();

            IReadOnlyList<TurnEffect> effects = SpecialRules.Tap(grid, 2, 2, random);

            ComboHelpers.AssertEffects(effects,
                "RocketCross 2,2 t=-1 c=. | 2,2H 3,2V 2,0G 2,1R 0,2B 1,2Y 4,2G 2,3Y 2,4B || m=2 cv=");
            SpecialGrid.AssertRows(new[] { "RG.YP", "GB.PR", ".....", "YP.GB", "PR.BY" }, grid);
            Assert.Equal(0, random.Calls);
        }

        // The center is the tapped cell, and the tapped cell comes first in Cleared.
        [Fact]
        public void Tap_Example1TapOtherRocket_CenteredOnTappedCellTappedFirst()
        {
            Grid grid = SpecialGrid.Parse(Example1);

            IReadOnlyList<TurnEffect> effects = SpecialRules.Tap(grid, 3, 2, new CountingRandom());

            ComboHelpers.AssertEffects(effects,
                "RocketCross 3,2 t=-1 c=. | 3,2V 2,2H 3,0B 3,1G 0,2B 1,2Y 4,2G 3,3P 3,4Y || m=2 cv=");
        }

        // Four neighbours: the others follow the tapped one in y then x order, not scan or BFS order.
        [Fact]
        public void Tap_FourNeighbours_AllJoinInYThenXOrder()
        {
            Grid grid = SpecialGrid.Parse("RGBYP", "GBVPR", "BHHHG", "YPVGB", "PRGBY");

            IReadOnlyList<TurnEffect> effects = SpecialRules.Tap(grid, 2, 2, new CountingRandom());

            ComboHelpers.AssertEffects(effects,
                "RocketCross 2,2 t=-1 c=. | 2,2H 2,1V 1,2H 3,2H 2,3V 2,0G 0,2B 4,2G 2,4B || m=5 cv=");
        }

        // (1,2) does not touch the tapped (0,0) but is linked through (1,0) and (1,1).
        [Fact]
        public void Tap_LinkedChainOfItems_FarItemJoinsCornerClipped()
        {
            Grid grid = SpecialGrid.Parse("RGBY", "GVBR", "BHYG", "HVRB");

            IReadOnlyList<TurnEffect> effects = SpecialRules.Tap(grid, 0, 0, new CountingRandom());

            ComboHelpers.AssertEffects(effects,
                "RocketCross 0,0 t=-1 c=. | 0,0H 1,0V 1,1H 1,2V 2,0R 3,0B 0,1B 0,2G 0,3R || m=4 cv=");
            SpecialGrid.AssertRows(new[] { ".GBY", "..BR", "..YG", "...." }, grid);
        }

        // Bombs touch the pair only diagonally and lie outside the cross: they neither join nor run.
        [Fact]
        public void Tap_DiagonalItems_DoNotJoin()
        {
            Grid grid = SpecialGrid.Parse("RGBY", "XBGX", "GHVB", "BRGR");

            IReadOnlyList<TurnEffect> effects = SpecialRules.Tap(grid, 1, 1, new CountingRandom());

            ComboHelpers.AssertEffects(effects,
                "RocketCross 1,1 t=-1 c=. | 1,1H 2,1V 1,0R 0,1G 3,1B 1,2B 1,3G || m=2 cv=");
            Assert.Equal(SpecialGrid.CellOf('X'), grid.Get(0, 2));
            Assert.Equal(SpecialGrid.CellOf('X'), grid.Get(3, 2));
        }

        // A colored or empty cell between two items breaks the link: the bomb does not join (no RocketBomb).
        [Theory]
        [InlineData("GVGX")]
        [InlineData("GV.X")]
        public void Tap_ColoredOrEmptyCellBetween_BreaksLink(string middleRow)
        {
            Grid grid = SpecialGrid.Parse("RGBY", middleRow, "BHRG");

            IReadOnlyList<TurnEffect> effects = SpecialRules.Tap(grid, 1, 0, new CountingRandom());

            ComboHelpers.AssertEffects(effects,
                "RocketCross 1,0 t=-1 c=. | 1,0H 1,1V 0,0B 2,0R 3,0G 1,2G || m=2 cv=");
            Assert.Equal(SpecialGrid.CellOf('X'), grid.Get(3, 1));
        }
    }
}
