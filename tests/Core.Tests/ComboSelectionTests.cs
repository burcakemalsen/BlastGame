using System.Collections.Generic;
using Xunit;

namespace BlastGame.Core.Tests
{
    /// <summary>
    /// Which combo applies (spec 06b, strength order): OrbOrb > OrbBomb > OrbRocket > BigBomb >
    /// RocketBomb > RocketCross, whatever the tapped item is. One-row boards; no red colored
    /// cells, so Orb conversions are empty.
    /// </summary>
    public class ComboSelectionTests
    {
        [Theory]
        [InlineData("BHVB", 1, EffectKind.RocketCross, 2)]
        [InlineData("BHHB", 2, EffectKind.RocketCross, 2)]
        [InlineData("BVVB", 1, EffectKind.RocketCross, 2)]
        [InlineData("BHXB", 1, EffectKind.RocketBomb, 2)]
        [InlineData("BXHB", 1, EffectKind.RocketBomb, 2)]
        [InlineData("BXXB", 1, EffectKind.BigBomb, 2)]
        [InlineData("BrHB", 1, EffectKind.OrbRocket, 2)]
        [InlineData("BHrB", 1, EffectKind.OrbRocket, 2)]
        [InlineData("BrXB", 1, EffectKind.OrbBomb, 2)]
        [InlineData("BXrB", 1, EffectKind.OrbBomb, 2)]
        [InlineData("BrgB", 1, EffectKind.OrbOrb, 2)]
        [InlineData("BrrB", 2, EffectKind.OrbOrb, 2)]
        [InlineData("BrHXB", 1, EffectKind.OrbBomb, 3)]
        [InlineData("BHXrB", 1, EffectKind.OrbBomb, 3)]
        [InlineData("BHXXB", 1, EffectKind.BigBomb, 3)]
        [InlineData("BHXHB", 1, EffectKind.RocketBomb, 3)]
        [InlineData("BrHHB", 2, EffectKind.OrbRocket, 3)]
        [InlineData("BrgHB", 3, EffectKind.OrbOrb, 3)]
        public void Tap_ParticipantKinds_StrongestComboFirstEffect(string row, int tapX, EffectKind kind, int merged)
        {
            Grid grid = SpecialGrid.Parse(row);

            IReadOnlyList<TurnEffect> effects = SpecialRules.Tap(grid, tapX, 0, CountingRandom.Constant(0));

            Assert.NotEmpty(effects);
            Assert.Equal(kind, effects[0].Kind);
            Assert.Equal(merged, effects[0].MergedCount);
            Assert.Equal(new CellPos(tapX, 0), effects[0].Origin);
            Assert.Equal(-1, effects[0].TriggeredBy);
            Assert.True(Cell.Empty == effects[0].Created);
            Assert.Equal(new CellPos(tapX, 0), effects[0].Cleared[0].Pos);
        }
    }
}
