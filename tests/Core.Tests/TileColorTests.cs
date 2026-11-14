using Xunit;

namespace BlastGame.Core.Tests
{
    public class TileColorTests
    {
        [Theory]
        [InlineData(TileColor.Red, 0)]
        [InlineData(TileColor.Green, 1)]
        [InlineData(TileColor.Blue, 2)]
        [InlineData(TileColor.Yellow, 3)]
        [InlineData(TileColor.Purple, 4)]
        [InlineData(TileColor.Orange, 5)]
        public void TileColor_Value_MatchesSpecOrder(TileColor color, int expected)
        {
            Assert.Equal(expected, (int)color);
        }

        [Fact]
        public void TileColor_HasExactlySixValues()
        {
            Assert.Equal(6, System.Enum.GetValues(typeof(TileColor)).Length);
        }
    }
}
