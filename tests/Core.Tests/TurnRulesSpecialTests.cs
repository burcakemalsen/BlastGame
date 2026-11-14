using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using static BlastGame.Core.Tests.TurnTestHelpers;

namespace BlastGame.Core.Tests
{
    /// <summary>
    /// TurnRules.Tap with special items, plus GroupFinder/BlastRules on special cells.
    /// Grids top row first (y grows upward); letters as in <see cref="SpecialGrid"/>.
    /// </summary>
    public class TurnRulesSpecialTests
    {
        private static readonly string[] Example1 = { "BBGGG", "RRRRR", "GBGBG" };
        private static readonly string[] Example1After = { "RRGRR", "BBVGG", "GBGBG" };
        private static readonly string[] Example3 = { "RGBGR", "GXHBG", "BRGRB" };

        private static int[] Repeat(int value, int count)
        {
            var list = new int[count];
            for (int i = 0; i < count; i++)
            {
                list[i] = value;
            }

            return list;
        }

        // ---------- spec examples ----------

        [Fact]
        public void Tap_Example1MergeToRocket_ExactEffectListsGridAndDraws()
        {
            Grid grid = SpecialGrid.Parse(Example1);
            var random = new CountingRandom(1, 0, 0, 0, 0);

            TurnResult result = TurnRules.Tap(grid, 2, 1, 3, random);

            SpecialGrid.AssertEffects(result.Effects, "Group 2,1 t=-1 c=V | 0,1R 1,1R 2,1R 3,1R 4,1R");
            TestGrid.AssertGroup(TestGrid.Pts(0, 1, 1, 1, 2, 1, 3, 1, 4, 1), result.Blasted);
            AssertMoves(new[] { 0, 2, 0, 1, 1, 2, 1, 1, 3, 2, 3, 1, 4, 2, 4, 1 }, result.Moves);
            TestGrid.AssertGroup(TestGrid.Pts(0, 2, 1, 2, 3, 2, 4, 2), result.Filled);
            SpecialGrid.AssertRows(Example1After, grid);
            Assert.Equal(new[] { 2, 3, 3, 3, 3 }, random.MaxArgs);
        }

        [Fact]
        public void Tap_Example2TapRocket_ClearsColumnRefillsWithoutDirectionDraw()
        {
            Grid grid = SpecialGrid.Parse(Example1After);
            var random = CountingRandom.Constant(0);

            TurnResult result = TurnRules.Tap(grid, 2, 1, 3, random);

            SpecialGrid.AssertEffects(result.Effects, "Rocket 2,1 t=-1 c=. | 2,1V 2,0G 2,2G");
            TestGrid.AssertGroup(TestGrid.Pts(2, 1, 2, 0, 2, 2), result.Blasted);
            AssertMoves(new int[0], result.Moves);
            TestGrid.AssertGroup(TestGrid.Pts(2, 0, 2, 1, 2, 2), result.Filled);
            SpecialGrid.AssertRows(new[] { "RRRRR", "BBRGG", "GBRBG" }, grid);
            Assert.Equal(new[] { 3, 3, 3 }, random.MaxArgs);
        }

        [Fact]
        public void Tap_Example3Chain_GravityAndRefillOnceAfterAllEffects()
        {
            Grid grid = SpecialGrid.Parse(Example3);
            var random = CountingRandom.Constant(0);

            TurnResult result = TurnRules.Tap(grid, 1, 1, 3, random);

            SpecialGrid.AssertEffects(result.Effects,
                "Bomb 1,1 t=-1 c=. | 1,1X 0,0B 1,0R 2,0G 0,1G 0,2R 1,2G 2,2B",
                "Rocket 2,1 t=0 c=. | 2,1H 3,1B 4,1G");
            TestGrid.AssertGroup(
                TestGrid.Pts(1, 1, 0, 0, 1, 0, 2, 0, 0, 1, 0, 2, 1, 2, 2, 2, 2, 1, 3, 1, 4, 1), result.Blasted);
            AssertMoves(new[] { 3, 2, 3, 1, 4, 2, 4, 1 }, result.Moves);
            TestGrid.AssertGroup(
                TestGrid.Pts(0, 0, 1, 0, 2, 0, 0, 1, 1, 1, 2, 1, 0, 2, 1, 2, 2, 2, 3, 2, 4, 2), result.Filled);
            SpecialGrid.AssertRows(new[] { "RRRRR", "RRRGR", "RRRRB" }, grid);
            Assert.Equal(Repeat(3, 11), random.MaxArgs);
        }

        // Example 6: a 9-cell green group gives a green orb; no direction draw, so the
        // first draw (2, invalid for Next(2)) belongs to refill.
        [Fact]
        public void Tap_Example6MergeToOrb_NoDirectionDrawOrbFalls()
        {
            Grid grid = SpecialGrid.Parse("GGGGG", "GGGGR");
            var random = CountingRandom.Constant(2);

            TurnResult result = TurnRules.Tap(grid, 2, 1, 3, random);

            SpecialGrid.AssertEffects(result.Effects,
                "Group 2,1 t=-1 c=g | 0,0G 1,0G 2,0G 3,0G 0,1G 1,1G 2,1G 3,1G 4,1G");
            AssertMoves(new[] { 2, 1, 2, 0 }, result.Moves);
            TestGrid.AssertGroup(TestGrid.Pts(0, 0, 1, 0, 3, 0, 0, 1, 1, 1, 2, 1, 3, 1, 4, 1), result.Filled);
            SpecialGrid.AssertRows(new[] { "BBBBB", "BBgBR" }, grid);
            Assert.Equal(Repeat(3, 8), random.MaxArgs);
        }

        // ---------- thresholds ----------

        // Top row: n red cells, then G/B alternating; bottom row Y/P alternating (5 colors).
        // Tap the rightmost red so the item is not born at the group's first cell.
        [Theory]
        [InlineData(4, -1, '.')]
        [InlineData(5, 0, 'H')]
        [InlineData(5, 1, 'V')]
        [InlineData(6, 1, 'V')]
        [InlineData(7, -1, 'X')]
        [InlineData(8, -1, 'X')]
        [InlineData(9, -1, 'r')]
        [InlineData(10, -1, 'r')]
        public void Tap_GroupSizeThreshold_ItemAtTappedCellAndDrawsAsSpecified(int n, int draw, char item)
        {
            var top = new StringBuilder();
            for (int x = 0; x < 10; x++)
            {
                top.Append(x < n ? 'R' : (x % 2 == 0 ? 'G' : 'B'));
            }

            Grid grid = SpecialGrid.Parse(top.ToString(), "YPYPYPYPYP");
            int fills = item == '.' ? n : n - 1;
            var values = new List<int>();
            if (draw >= 0)
            {
                values.Add(draw);
            }

            values.AddRange(Repeat(0, fills));
            var random = new CountingRandom(values.ToArray());

            TurnResult result = TurnRules.Tap(grid, n - 1, 1, 5, random);

            var cleared = new List<string>();
            for (int x = 0; x < n; x++)
            {
                cleared.Add(x + ",1R");
            }

            SpecialGrid.AssertEffects(result.Effects,
                "Group " + (n - 1) + ",1 t=-1 c=" + item + " | " + string.Join(" ", cleared));
            Assert.Equal(n, result.Blasted.Count);
            Assert.Empty(result.Moves);
            Assert.Equal(fills, result.Filled.Count);
            Assert.Equal(SpecialGrid.CellOf(item == '.' ? 'R' : item), grid.Get(n - 1, 1));
            var expectedArgs = new List<int>();
            if (draw >= 0)
            {
                expectedArgs.Add(2);
            }

            expectedArgs.AddRange(Repeat(5, fills));
            Assert.Equal(expectedArgs, random.MaxArgs);
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(2)]
        public void Tap_InvalidDirectionDraw_ThrowsGridUnchangedCellByCell(int draw)
        {
            Grid grid = SpecialGrid.Parse(Example1);
            Grid twin = Clone(grid);
            var random = new CountingRandom(draw, 0, 0, 0, 0);

            Assert.Throws<InvalidOperationException>(() => TurnRules.Tap(grid, 2, 1, 3, random));
            SpecialGrid.AssertSameCells(twin, grid);
            Assert.Equal(1, random.Calls);
        }

        [Fact]
        public void Tap_MergeAtTopOfColumn_ItemBornAtTappedCellThenFalls()
        {
            Grid grid = SpecialGrid.Parse("RG", "RB", "RG", "RB", "RG");
            var random = new CountingRandom(1, 0, 0, 0, 0);

            TurnResult result = TurnRules.Tap(grid, 0, 4, 3, random);

            SpecialGrid.AssertEffects(result.Effects, "Group 0,4 t=-1 c=V | 0,0R 0,1R 0,2R 0,3R 0,4R");
            TestGrid.AssertGroup(TestGrid.Pts(0, 0, 0, 1, 0, 2, 0, 3, 0, 4), result.Blasted);
            AssertMoves(new[] { 0, 4, 0, 0 }, result.Moves);
            TestGrid.AssertGroup(TestGrid.Pts(0, 1, 0, 2, 0, 3, 0, 4), result.Filled);
            SpecialGrid.AssertRows(new[] { "RG", "RB", "RG", "RB", "VG" }, grid);
        }

        // ---------- plain taps keep working, with effects ----------

        [Fact]
        public void Tap_GroupOfThree_SingleGroupEffectNoItem()
        {
            Grid grid = SpecialGrid.Parse("BGG", "RRG", "BRB");

            TurnResult result = TurnRules.Tap(grid, 1, 0, 3, CountingRandom.Constant(2));

            SpecialGrid.AssertEffects(result.Effects, "Group 1,0 t=-1 c=. | 1,0R 0,1R 1,1R");
            Assert.Equal(SpecialGrid.ClearedPositions(result.Effects), result.Blasted);
        }

        [Theory]
        [InlineData(1, 1)]   // empty
        [InlineData(1, 0)]   // single colored cell
        public void Tap_NoBlast_EffectsEmptyAndSourceNotCalled(int x, int y)
        {
            Grid grid = SpecialGrid.Parse("R.", "RG");
            var random = CountingRandom.Constant(0);

            TurnResult result = TurnRules.Tap(grid, x, y, 3, random);

            Assert.False(result.HasBlasted);
            Assert.NotNull(result.Effects);
            Assert.Empty(result.Effects);
            Assert.Equal(0, random.Calls);
        }

        [Fact]
        public void Tap_ColoredGroupNextToSameColorOrb_OrbNotPartOfGroup()
        {
            Grid grid = SpecialGrid.Parse("RRrRR");

            TurnResult result = TurnRules.Tap(grid, 0, 0, 3, CountingRandom.Constant(1));

            SpecialGrid.AssertEffects(result.Effects, "Group 0,0 t=-1 c=. | 0,0R 1,0R");
            SpecialGrid.AssertRows(new[] { "GGrRR" }, grid);
        }

        [Fact]
        public void Tap_OrbWithNothingToClear_StillBlastsItselfAndRefills()
        {
            Grid grid = SpecialGrid.Parse("BrG", "GBY");

            TurnResult result = TurnRules.Tap(grid, 1, 1, 5, CountingRandom.Constant(4));

            Assert.True(result.HasBlasted);
            SpecialGrid.AssertEffects(result.Effects, "Orb 1,1 t=-1 c=. | 1,1r");
            TestGrid.AssertGroup(TestGrid.Pts(1, 1), result.Blasted);
            TestGrid.AssertGroup(TestGrid.Pts(1, 1), result.Filled);
            SpecialGrid.AssertRows(new[] { "BPG", "GBY" }, grid);
        }

        [Fact]
        public void Tap_ChainLongerThanTwo_BlastedIsClearedPositionsInEffectOrder()
        {
            Grid grid = SpecialGrid.Parse("RGBXY", "GHBGR", "BRXGY", "YGBVG", "RBGPR");

            TurnResult result = TurnRules.Tap(grid, 2, 2, 5, CountingRandom.Constant(0));

            Assert.Equal(4, result.Effects.Count);
            Assert.Equal(SpecialGrid.ClearedPositions(result.Effects), result.Blasted);
            SpecialGrid.AssertClearedUnique(result.Effects);
        }

        // ---------- checks still come first ----------

        [Fact]
        public void Tap_NullRandomOnSpecial_ThrowsGridUntouched()
        {
            Grid grid = SpecialGrid.Parse(Example3);
            Grid twin = Clone(grid);

            Assert.Throws<ArgumentNullException>(() => TurnRules.Tap(grid, 1, 1, 3, null));
            SpecialGrid.AssertSameCells(twin, grid);
        }

        [Fact]
        public void Tap_BadColorCountOnSpecial_ThrowsGridAndSourceUntouched()
        {
            Grid grid = SpecialGrid.Parse(Example3);
            Grid twin = Clone(grid);
            var random = CountingRandom.Constant(0);

            Assert.Throws<ArgumentOutOfRangeException>(() => TurnRules.Tap(grid, 1, 1, 0, random));
            SpecialGrid.AssertSameCells(twin, grid);
            Assert.Equal(0, random.Calls);
        }

        // ---------- reproducibility ----------

        [Fact]
        public void Tap_SameSeedSameTapsWithSpecials_SameGridsAndEffects()
        {
            var randomA = new SeededRandom(2026);
            var randomB = new SeededRandom(2026);
            Grid a = GridFactory.CreateRandom(9, 9, 3, randomA);
            Grid b = GridFactory.CreateRandom(9, 9, 3, randomB);
            int specialEffects = 0;

            for (int i = 0; i < 60; i++)
            {
                int x = (i * 7) % 9;
                int y = (i * 5 + 2) % 9;
                if (i % 2 == 1)
                {
                    FirstSpecial(a, ref x, ref y);
                }

                TurnResult ra = TurnRules.Tap(a, x, y, 3, randomA);
                TurnResult rb = TurnRules.Tap(b, x, y, 3, randomB);

                Assert.Equal(SpecialGrid.Dump(a), SpecialGrid.Dump(b));
                Assert.Equal(SpecialGrid.Format(ra.Effects), SpecialGrid.Format(rb.Effects));
                Assert.Equal(Format(ra.Moves), Format(rb.Moves));
                Assert.Equal(Format(ra.Filled), Format(rb.Filled));
                foreach (TurnEffect e in ra.Effects)
                {
                    if (e.Kind != EffectKind.Group)
                    {
                        specialEffects++;
                    }
                }
            }

            Assert.True(specialEffects > 0, "No special item ran, so the test proves nothing.");
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

        // ---------- GroupFinder / BlastRules on special cells ----------

        [Theory]
        [InlineData(1, 1)]   // rocket
        [InlineData(0, 0)]   // bomb
        [InlineData(1, 0)]   // orb
        public void FindGroupAndBlast_OnSpecial_EmptyListGridUntouched(int x, int y)
        {
            string[] rows = { "RHR", "XrR" };
            Grid grid = SpecialGrid.Parse(rows);

            Assert.Empty(GroupFinder.FindGroup(grid, x, y));
            Assert.Empty(BlastRules.Blast(grid, x, y));
            SpecialGrid.AssertRows(rows, grid);
        }

        [Fact]
        public void FindGroupAndBlast_GroupNextToSameColorOrb_OrbExcludedAndNotCrossed()
        {
            Grid grid = SpecialGrid.Parse("RRrRR");

            TestGrid.AssertGroup(TestGrid.Pts(0, 0, 1, 0), GroupFinder.FindGroup(grid, 0, 0));
            TestGrid.AssertGroup(TestGrid.Pts(0, 0, 1, 0), BlastRules.Blast(grid, 0, 0));
            SpecialGrid.AssertRows(new[] { "..rRR" }, grid);
        }
    }
}
