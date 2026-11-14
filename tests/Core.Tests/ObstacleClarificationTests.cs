using Xunit;

namespace BlastGame.Core.Tests
{
    /// <summary>Spec 07, later clarifications: Orb own cell, conversion, where hits apply, FindGroup, RocketBomb.</summary>
    public class ObstacleClarificationTests
    {
        [Fact]
        public void Tap_SingleOrb_HitsObstacleNextToItsOwnCell()
        {
            Grid grid = ObstacleGrid.Parse("P *R G");

            TurnResult result = TurnRules.Tap(grid, 1, 0, 3, CountingRandom.Constant(1));

            Assert.Equal("1,0*R", ObstacleGrid.Cleared(result.Effects[0]));
            ObstacleGrid.AssertHits(result.Effects[0], "0,0 P>.");
        }

        [Fact]
        public void Tap_OrbRocketConversion_DoesNotHitNeighborOfConvertedSquare()
        {
            Grid grid = ObstacleGrid.Parse(
                "*R H B G",
                "G B P G",
                "B G R B");

            TurnResult result = TurnRules.Tap(grid, 0, 2, 3, CountingRandom.Constant(0));

            Assert.Equal("2,0R", ObstacleGrid.Converted(result.Effects[0]));
            Assert.Equal("2,0H 1,0G 3,0B 0,0B", ObstacleGrid.Cleared(result.Effects[1]));
            Assert.Equal(new[] { new string[0], new string[0] }, ObstacleGrid.HitsPerEffect(result));
            Assert.Equal("P", ObstacleGrid.TokenOf(grid.Get(2, 0)));
        }

        [Fact]
        public void Activate_RocketThroughCrate_FillsHitsAndAppliesToGrid()
        {
            Grid grid = ObstacleGrid.Parse("H C2 R");

            var effects = SpecialRules.Activate(grid, 0, 0);

            ObstacleGrid.AssertHits(effects[0], "1,0 C2>C1");
            ObstacleGrid.AssertRows(grid, ". C1 .");
        }

        [Fact]
        public void SpecialRulesTap_ComboThroughCrate_FillsHitsAndAppliesToGrid()
        {
            Grid grid = ObstacleGrid.Parse("H V C2 R");

            var effects = SpecialRules.Tap(grid, 0, 0, CountingRandom.Constant(0));

            Assert.Equal(EffectKind.RocketCross, effects[0].Kind);
            ObstacleGrid.AssertHits(effects[0], "2,0 C2>C1");
            ObstacleGrid.AssertRows(grid, ". . C1 .");
        }

        [Fact]
        public void Blast_GroupNextToObstacles_DoesNotHitThem()
        {
            Grid grid = ObstacleGrid.Parse("P R R C2");

            var blasted = BlastRules.Blast(grid, 1, 0);

            Assert.Equal(2, blasted.Count);
            ObstacleGrid.AssertRows(grid, "P . . C2");
        }

        [Theory]
        [InlineData(0)] // Pinata
        [InlineData(1)] // Crate
        [InlineData(2)] // Anchor
        public void FindGroup_FromObstacleCell_ReturnsEmpty(int x)
        {
            Grid grid = ObstacleGrid.Parse("P C2 A", "R R R");

            Assert.Empty(GroupFinder.FindGroup(grid, x, 1));
        }

        [Fact]
        public void Tap_RocketBomb_HitsObstaclesInThreeRowsAndColumnsOnly()
        {
            Grid grid = ObstacleGrid.Parse(
                "G P B G C3",
                "B G R G C2",
                "R B X B R",
                "G R H R G",
                "C1 G B G B");

            TurnResult result = TurnRules.Tap(grid, 2, 2, 3, CountingRandom.Constant(0));

            Assert.Single(result.Effects);
            Assert.Equal(EffectKind.RocketBomb, result.Effects[0].Kind);
            ObstacleGrid.AssertHits(result.Effects[0], "4,3 C2>C1", "1,4 P>.");
            Assert.Equal("C1", ObstacleGrid.TokenOf(grid.Get(0, 0)));
            Assert.Equal("C3", ObstacleGrid.TokenOf(grid.Get(4, 4)));
        }
    }
}
