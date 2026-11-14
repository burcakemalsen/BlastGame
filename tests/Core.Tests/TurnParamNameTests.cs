using System;
using Xunit;

namespace BlastGame.Core.Tests
{
    /// <summary>
    /// Order of two errors of the same type, told apart by ParamName
    /// (spec 03, Fill and Tap error rules), plus a no-blast tap on a grid
    /// with holes.
    /// </summary>
    public class TurnParamNameTests
    {
        private static readonly string[] Example3 = { "BGG", "RRG", "BRB" };

        [Fact]
        public void Fill_NullGridAndNullRandom_ParamNameGrid()
        {
            var ex = Assert.Throws<ArgumentNullException>(() => RefillRules.Fill(null, 3, null));
            Assert.Equal("grid", ex.ParamName);
        }

        [Fact]
        public void Tap_NullGridAndNullRandom_ParamNameGrid()
        {
            var ex = Assert.Throws<ArgumentNullException>(() => TurnRules.Tap(null, 0, 0, 3, null));
            Assert.Equal("grid", ex.ParamName);
        }

        [Fact]
        public void Tap_BadColorCountAndCoordinateOutside_ParamNameColorCount()
        {
            Grid grid = TestGrid.Parse(Example3);
            var random = CountingRandom.Constant(0);

            var ex = Assert.Throws<ArgumentOutOfRangeException>(() => TurnRules.Tap(grid, 9, 9, 0, random));

            Assert.Equal("colorCount", ex.ParamName);
            Assert.Equal(0, random.Calls);
            TestGrid.AssertRows(Example3, grid);
        }

        [Theory]
        [InlineData(-1, -1)]
        [InlineData(3, 3)]
        public void Tap_XAndYBothOutside_ParamNameX(int x, int y)
        {
            Grid grid = TestGrid.Parse(Example3);
            var ex = Assert.Throws<ArgumentOutOfRangeException>(
                () => TurnRules.Tap(grid, x, y, 3, CountingRandom.Constant(0)));
            Assert.Equal("x", ex.ParamName);
        }

        [Fact]
        public void Tap_OnlyYOutside_ParamNameY()
        {
            Grid grid = TestGrid.Parse(Example3);
            var ex = Assert.Throws<ArgumentOutOfRangeException>(
                () => TurnRules.Tap(grid, 1, 3, 3, CountingRandom.Constant(0)));
            Assert.Equal("y", ex.ParamName);
        }

        [Fact]
        public void Tap_SingleCellOnGridWithHoles_HolesStaySourceNotCalled()
        {
            // (1,1) is Y with no Y neighbour; the grid has empty cells that
            // Fill would fill if it were called without a blast.
            string[] rows = { "R.G", "BYG", ".RB" };
            Grid grid = TestGrid.Parse(rows);
            var random = CountingRandom.Constant(0);

            TurnResult result = TurnRules.Tap(grid, 1, 1, 3, random);

            Assert.Empty(result.Blasted);
            Assert.Empty(result.Moves);
            Assert.Empty(result.Filled);
            Assert.Equal(0, random.Calls);
            TestGrid.AssertRows(rows, grid);
        }
    }
}
