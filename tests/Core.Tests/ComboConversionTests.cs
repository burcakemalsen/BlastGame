using System;
using System.Collections.Generic;
using Xunit;
using static BlastGame.Core.Tests.TurnTestHelpers;

namespace BlastGame.Core.Tests
{
    /// <summary>
    /// OrbRocket / OrbBomb (spec 06b): the Orb's colored cells turn into items (Converted, y then x),
    /// all queued with TriggeredBy 0, then each runs alone. OrbRocket draws one Next(2) per converted
    /// cell before the grid changes; OrbBomb draws nothing. Grids top row first (y grows upward).
    /// </summary>
    public class ComboConversionTests
    {
        private static readonly string[] Example5 = { "RBRB", "BrHB", "RBBB" };

        [Fact]
        public void Tap_Example5OrbRocket_ExactEffectsDrawsAndGrid()
        {
            Grid grid = SpecialGrid.Parse(Example5);
            var random = new CountingRandom(1, 0, 1);

            IReadOnlyList<TurnEffect> effects = SpecialRules.Tap(grid, 1, 1, random);

            ComboHelpers.AssertEffects(effects,
                "OrbRocket 1,1 t=-1 c=. | 1,1r 2,1H || m=2 cv=0,0R 0,2R 2,2R",
                "Rocket 0,0 t=0 c=. | 0,0V 0,1B || m=1 cv=",
                "Rocket 0,2 t=0 c=. | 0,2H 1,2B 3,2B || m=1 cv=",
                "Rocket 2,2 t=0 c=. | 2,2V 2,0B || m=1 cv=");
            SpecialGrid.AssertRows(new[] { "....", "...B", ".B.B" }, grid);
            Assert.Equal(new[] { 2, 2, 2 }, random.MaxArgs);
            SpecialGrid.AssertClearedUnique(effects);
        }

        // Example 6: the bomb joins through the rocket; Orb + Bomb beats Orb + Rocket.
        [Fact]
        public void Tap_Example6ThreeParticipants_OrbBombExactEffectsNoDraw()
        {
            Grid grid = SpecialGrid.Parse("GBGB", "BRXG", "RrHB", "GRBR");
            var random = new CountingRandom();

            IReadOnlyList<TurnEffect> effects = SpecialRules.Tap(grid, 1, 1, random);

            ComboHelpers.AssertEffects(effects,
                "OrbBomb 1,1 t=-1 c=. | 1,1r 2,1H 2,2X || m=3 cv=1,0R 3,0R 0,1R 1,2R",
                "Bomb 1,0 t=0 c=. | 1,0X 0,0G 2,0B || m=1 cv=",
                "Bomb 3,0 t=0 c=. | 3,0X 3,1B || m=1 cv=",
                "Bomb 0,1 t=0 c=. | 0,1X 0,2B || m=1 cv=",
                "Bomb 1,2 t=0 c=. | 1,2X 0,3G 1,3B 2,3G || m=1 cv=");
            SpecialGrid.AssertRows(new[] { "...B", "...G", "....", "...." }, grid);
            Assert.Equal(0, random.Calls);
            SpecialGrid.AssertClearedUnique(effects);
        }

        [Theory]
        [InlineData('H', EffectKind.OrbRocket)]
        [InlineData('X', EffectKind.OrbBomb)]
        public void Tap_NoCellOfOrbColor_EmptyConversionOnlyParticipantsNoDraw(char item, EffectKind kind)
        {
            Grid grid = SpecialGrid.Parse("BGB", "r" + item + "G");
            var random = new CountingRandom();

            IReadOnlyList<TurnEffect> effects = SpecialRules.Tap(grid, 0, 0, random);

            ComboHelpers.AssertEffects(effects, kind + " 0,0 t=-1 c=. | 0,0r 1,0" + item + " || m=2 cv=");
            SpecialGrid.AssertRows(new[] { "BGB", "..G" }, grid);
            Assert.Equal(0, random.Calls);
        }

        // Five red cells: five draws of Next(2), in Converted order; each rocket's direction follows its draw.
        [Fact]
        public void Tap_OrbRocketFiveCells_OneDrawPerCellInConvertedOrder()
        {
            Grid grid = ComboHelpers.Board(5, 5, "GBY", "0,0,r", "1,0,H", "4,0,R", "2,1,R", "0,3,R", "3,3,R", "4,4,R");
            var random = new CountingRandom(0, 1, 1, 0, 1);

            IReadOnlyList<TurnEffect> effects = SpecialRules.Tap(grid, 0, 0, random);

            Assert.Equal("4,0R 2,1R 0,3R 3,3R 4,4R", SpecialGrid.Format(effects[0].Converted));
            Assert.Equal(new[]
            {
                "OrbRocket 0,0 t=-1", "Rocket 4,0 t=0", "Rocket 2,1 t=0", "Rocket 0,3 t=0", "Rocket 3,3 t=0", "Rocket 4,4 t=0"
            }, ComboHelpers.Heads(effects));
            var directions = new List<string>();
            for (int i = 1; i < effects.Count; i++)
            {
                directions.Add(SpecialGrid.Format(new[] { effects[i].Cleared[0] }));
            }

            Assert.Equal(new[] { "4,0H", "2,1V", "0,3V", "3,3H", "4,4V" }, directions);
            Assert.Equal(new[] { 2, 2, 2, 2, 2 }, random.MaxArgs);
            SpecialGrid.AssertClearedUnique(effects);
        }

        // A converted cell never shows up as a colored cell in any Cleared; it appears once, as an item.
        [Fact]
        public void Tap_ConvertedCells_NeverClearedAsColor()
        {
            Grid grid = ComboHelpers.Board(5, 5, "GBY", "0,0,r", "1,0,X", "4,0,R", "2,1,R", "2,2,R", "0,3,R", "4,4,R");

            IReadOnlyList<TurnEffect> effects = SpecialRules.Tap(grid, 0, 0, new CountingRandom());

            Assert.Equal(EffectKind.OrbBomb, effects[0].Kind);
            Assert.Equal(5, effects[0].Converted.Count);
            Assert.Equal(6, effects.Count);
            foreach (ClearedCell converted in effects[0].Converted)
            {
                foreach (TurnEffect e in effects)
                {
                    foreach (ClearedCell c in e.Cleared)
                    {
                        if (c.Pos == converted.Pos)
                        {
                            Assert.True(c.Cell == Cell.OfSpecial(SpecialKind.Bomb), "Converted " + c.Pos + " cleared as " + c.Cell);
                            Assert.Equal(converted.Pos, e.Origin);
                        }
                    }
                }
            }

            SpecialGrid.AssertClearedUnique(effects);
        }

        // The tapped item is a rocket; the color comes from the Orb next to it.
        [Fact]
        public void Tap_TappedRocketNextToBlueOrb_ConvertsBlueCellsOriginIsTappedCell()
        {
            Grid grid = SpecialGrid.Parse("BRBR", "HbRB");

            IReadOnlyList<TurnEffect> effects = SpecialRules.Tap(grid, 0, 0, CountingRandom.Constant(0));

            Assert.Equal(EffectKind.OrbRocket, effects[0].Kind);
            Assert.Equal(new CellPos(0, 0), effects[0].Origin);
            Assert.Equal("0,0H 1,0b", SpecialGrid.Format(effects[0].Cleared));
            Assert.Equal("3,0B 0,1B 2,1B", SpecialGrid.Format(effects[0].Converted));
            Assert.Equal(4, effects.Count);
        }

        // Another red Orb on the board is not a colored cell: not converted, and out of reach it stays.
        [Fact]
        public void Tap_SameColorOrbElsewhere_NotConverted()
        {
            Grid grid = SpecialGrid.Parse("rBBB", "BBRB", "rXBB");

            IReadOnlyList<TurnEffect> effects = SpecialRules.Tap(grid, 0, 0, new CountingRandom());

            Assert.Equal("2,1R", SpecialGrid.Format(effects[0].Converted));
            Assert.Equal(2, effects.Count);
            Assert.Equal(SpecialGrid.CellOf('r'), grid.Get(0, 2));
        }

        // A converted bomb reaches a rocket that is not a participant: it runs alone, after all converted items.
        [Fact]
        public void Tap_ConvertedBombReachesOtherItem_QueuedAfterTriggeredByThatBomb()
        {
            Grid grid = SpecialGrid.Parse("BBBBH", "rXBRB", "BBBBB");

            IReadOnlyList<TurnEffect> effects = SpecialRules.Tap(grid, 0, 1, new CountingRandom());

            ComboHelpers.AssertEffects(effects,
                "OrbBomb 0,1 t=-1 c=. | 0,1r 1,1X || m=2 cv=3,1R",
                "Bomb 3,1 t=0 c=. | 3,1X 2,0B 3,0B 4,0B 2,1B 4,1B 2,2B 3,2B || m=1 cv=",
                "Rocket 4,2 t=1 c=. | 4,2H 1,2B 0,2B || m=1 cv=");
            SpecialGrid.AssertRows(new[] { ".....", ".....", "BB..." }, grid);
        }

        [Theory]
        [InlineData(2, 0, 0)]
        [InlineData(-1, 0, 0)]
        [InlineData(1, 0, 5)]    // only the last draw is bad
        public void Tap_InvalidDirectionDraw_ThrowsGridUnchangedCellByCell(int a, int b, int c)
        {
            Grid grid = SpecialGrid.Parse(Example5);
            Grid twin = Clone(grid);

            Assert.Throws<InvalidOperationException>(() => SpecialRules.Tap(grid, 1, 1, new CountingRandom(a, b, c)));
            SpecialGrid.AssertSameCells(twin, grid);
        }
    }
}
