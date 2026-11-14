using System;
using Xunit;
using static BlastGame.Core.Tests.TurnTestHelpers;

namespace BlastGame.Core.Tests
{
    /// <summary>
    /// TurnRules.Tap on a combo (spec 06b): Blasted = Cleared positions in effect order, gravity and
    /// refill once after every effect, OrbRocket direction draws before refill draws.
    /// Grids top row first (y grows upward).
    /// </summary>
    public class TurnRulesComboTests
    {
        private static readonly string[] Example5 = { "RBRB", "BrHB", "RBBB" };

        private static int[] Repeat(int value, int count)
        {
            var list = new int[count];
            for (int i = 0; i < count; i++)
            {
                list[i] = value;
            }

            return list;
        }

        private static int[] Concat(int[] a, int[] b)
        {
            var list = new int[a.Length + b.Length];
            a.CopyTo(list, 0);
            b.CopyTo(list, a.Length);
            return list;
        }

        [Fact]
        public void Tap_Example1RocketCross_BlastedGravityRefillNoDirectionDraw()
        {
            Grid grid = SpecialGrid.Parse("RGBYP", "GBYPR", "BYHVG", "YPRGB", "PRGBY");
            var random = CountingRandom.Constant(0);

            TurnResult result = TurnRules.Tap(grid, 2, 2, 5, random);

            Assert.True(result.HasBlasted);
            ComboHelpers.AssertEffects(result.Effects,
                "RocketCross 2,2 t=-1 c=. | 2,2H 3,2V 2,0G 2,1R 0,2B 1,2Y 4,2G 2,3Y 2,4B || m=2 cv=");
            TestGrid.AssertGroup(TestGrid.Pts(2, 2, 3, 2, 2, 0, 2, 1, 0, 2, 1, 2, 4, 2, 2, 3, 2, 4), result.Blasted);
            AssertMoves(new[] { 0, 3, 0, 2, 0, 4, 0, 3, 1, 3, 1, 2, 1, 4, 1, 3, 3, 3, 3, 2, 3, 4, 3, 3, 4, 3, 4, 2, 4, 4, 4, 3 },
                result.Moves);
            TestGrid.AssertGroup(TestGrid.Pts(2, 0, 2, 1, 2, 2, 2, 3, 0, 4, 1, 4, 2, 4, 3, 4, 4, 4), result.Filled);
            SpecialGrid.AssertRows(new[] { "RRRRR", "RGRYP", "GBRPR", "YPRGB", "PRRBY" }, grid);
            Assert.Equal(Repeat(5, 9), random.MaxArgs);
        }

        // Example 5 with source 1, 0, 1 then zeros: three Next(2) first, then nine Next(3) refill draws.
        [Fact]
        public void Tap_Example5OrbRocket_DirectionDrawsBeforeRefill()
        {
            Grid grid = SpecialGrid.Parse(Example5);
            var random = new CountingRandom(Concat(new[] { 1, 0, 1 }, Repeat(0, 9)));

            TurnResult result = TurnRules.Tap(grid, 1, 1, 3, random);

            ComboHelpers.AssertEffects(result.Effects,
                "OrbRocket 1,1 t=-1 c=. | 1,1r 2,1H || m=2 cv=0,0R 0,2R 2,2R",
                "Rocket 0,0 t=0 c=. | 0,0V 0,1B || m=1 cv=",
                "Rocket 0,2 t=0 c=. | 0,2H 1,2B 3,2B || m=1 cv=",
                "Rocket 2,2 t=0 c=. | 2,2V 2,0B || m=1 cv=");
            TestGrid.AssertGroup(TestGrid.Pts(1, 1, 2, 1, 0, 0, 0, 1, 0, 2, 1, 2, 3, 2, 2, 2, 2, 0), result.Blasted);
            AssertMoves(new int[0], result.Moves);
            TestGrid.AssertGroup(TestGrid.Pts(0, 0, 2, 0, 0, 1, 1, 1, 2, 1, 0, 2, 1, 2, 2, 2, 3, 2), result.Filled);
            SpecialGrid.AssertRows(new[] { "RRRR", "RRRB", "RBRB" }, grid);
            Assert.Equal(Concat(Repeat(2, 3), Repeat(3, 9)), random.MaxArgs);
        }

        // Step 6 Example 3 (bomb next to rocket) is now a RocketBomb: on a 3-high board it takes everything.
        [Fact]
        public void Tap_Step6Example3BombNextToRocket_NowRocketBombWholeBoard()
        {
            Grid grid = SpecialGrid.Parse("RGBGR", "GXHBG", "BRGRB");
            var random = CountingRandom.Constant(0);

            TurnResult result = TurnRules.Tap(grid, 1, 1, 3, random);

            ComboHelpers.AssertEffects(result.Effects,
                "RocketBomb 1,1 t=-1 c=. | 1,1X 2,1H 0,0B 1,0R 2,0G 3,0R 4,0B 0,1G 3,1B 4,1G 0,2R 1,2G 2,2B 3,2G 4,2R || m=2 cv=");
            Assert.Equal(SpecialGrid.ClearedPositions(result.Effects), result.Blasted);
            Assert.Empty(result.Moves);
            Assert.Equal(15, result.Filled.Count);
            SpecialGrid.AssertRows(new[] { "RRRRR", "RRRRR", "RRRRR" }, grid);
            Assert.Equal(Repeat(3, 15), random.MaxArgs);
        }

        // Example 6: combo plus four chained bombs, then one gravity pass and one refill; OrbBomb draws nothing.
        [Fact]
        public void Tap_Example6OrbBombChain_GravityAndRefillOnceAfterAllEffects()
        {
            Grid grid = SpecialGrid.Parse("GBGB", "BRXG", "RrHB", "GRBR");
            var random = CountingRandom.Constant(0);

            TurnResult result = TurnRules.Tap(grid, 1, 1, 3, random);

            Assert.Equal(5, result.Effects.Count);
            Assert.Equal(SpecialGrid.ClearedPositions(result.Effects), result.Blasted);
            AssertMoves(new[] { 3, 2, 3, 0, 3, 3, 3, 1 }, result.Moves);
            TestGrid.AssertGroup(
                TestGrid.Pts(0, 0, 1, 0, 2, 0, 0, 1, 1, 1, 2, 1, 0, 2, 1, 2, 2, 2, 3, 2, 0, 3, 1, 3, 2, 3, 3, 3),
                result.Filled);
            SpecialGrid.AssertRows(new[] { "RRRR", "RRRR", "RRRB", "RRRG" }, grid);
            Assert.Equal(Repeat(3, 14), random.MaxArgs);
        }

        [Fact]
        public void Tap_OrbRocketInvalidDirectionDraw_ThrowsGridUnchangedCellByCell()
        {
            Grid grid = SpecialGrid.Parse(Example5);
            Grid twin = Clone(grid);

            Assert.Throws<InvalidOperationException>(() => TurnRules.Tap(grid, 1, 1, 3, new CountingRandom(1, 2, 0, 0)));
            SpecialGrid.AssertSameCells(twin, grid);
        }
    }
}
