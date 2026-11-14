using Xunit;
using static BlastGame.Core.Tests.TurnTestHelpers;

namespace BlastGame.Core.Tests
{
    /// <summary>Same seed, same taps → same grids and effects, with real combos (spec 06b).</summary>
    public class ComboReproducibilityTests
    {
        private static Grid Planted(SeededRandom random)
        {
            Grid grid = GridFactory.CreateRandom(9, 9, 3, random);
            grid.Set(1, 1, Cell.OfOrb(TileColor.Red));
            grid.Set(2, 1, Cell.OfSpecial(SpecialKind.RocketHorizontal));
            grid.Set(5, 6, Cell.OfSpecial(SpecialKind.Bomb));
            grid.Set(5, 7, Cell.OfSpecial(SpecialKind.Bomb));
            grid.Set(7, 3, Cell.OfSpecial(SpecialKind.RocketVertical));
            grid.Set(7, 4, Cell.OfSpecial(SpecialKind.Bomb));
            return grid;
        }

        [Fact]
        public void Tap_SameSeedSameTapsWithCombos_SameGridsAndEffects()
        {
            var randomA = new SeededRandom(606);
            var randomB = new SeededRandom(606);
            Grid a = Planted(randomA);
            Grid b = Planted(randomB);
            int combos = 0;

            for (int i = 0; i < 40; i++)
            {
                int x = (i * 7) % 9;
                int y = (i * 5 + 2) % 9;
                if (i == 0)
                {
                    x = 1;
                    y = 1;
                }
                else if (i % 2 == 1)
                {
                    FirstSpecial(a, ref x, ref y);
                }

                TurnResult ra = TurnRules.Tap(a, x, y, 3, randomA);
                TurnResult rb = TurnRules.Tap(b, x, y, 3, randomB);

                Assert.Equal(SpecialGrid.Dump(a), SpecialGrid.Dump(b));
                Assert.Equal(ComboHelpers.Format(ra.Effects), ComboHelpers.Format(rb.Effects));
                Assert.Equal(Format(ra.Blasted), Format(rb.Blasted));
                Assert.Equal(Format(ra.Moves), Format(rb.Moves));
                Assert.Equal(Format(ra.Filled), Format(rb.Filled));
                foreach (TurnEffect e in ra.Effects)
                {
                    if (e.MergedCount >= 2)
                    {
                        combos++;
                    }
                }
            }

            Assert.True(combos > 0, "No combo happened, so the test proves nothing.");
        }

        private static void FirstSpecial(Grid grid, ref int x, ref int y)
        {
            for (int cy = 0; cy < grid.Height; cy++)
            {
                for (int cx = 0; cx < grid.Width; cx++)
                {
                    if (grid.Get(cx, cy).Kind == CellKind.Special)
                    {
                        x = cx;
                        y = cy;
                        return;
                    }
                }
            }
        }
    }
}
