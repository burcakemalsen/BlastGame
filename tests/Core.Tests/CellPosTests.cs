using System.Collections.Generic;
using Xunit;

namespace BlastGame.Core.Tests
{
    public class CellPosTests
    {
        [Theory]
        [InlineData(0, 0)]
        [InlineData(3, 4)]
        [InlineData(4, 3)]
        [InlineData(15, 0)]
        [InlineData(0, 15)]
        [InlineData(-1, -2)]
        [InlineData(int.MinValue, int.MaxValue)]
        [InlineData(int.MaxValue, int.MinValue)]
        public void Constructor_AnyValues_XAndYAreStoredInOrder(int x, int y)
        {
            var pos = new CellPos(x, y);

            Assert.Equal(x, pos.X);
            Assert.Equal(y, pos.Y);
        }

        [Fact]
        public void Default_IsOrigin()
        {
            CellPos pos = default;

            Assert.Equal(0, pos.X);
            Assert.Equal(0, pos.Y);
        }

        [Fact]
        public void Equals_SameCoordinates_ReturnsTrue()
        {
            Assert.True(new CellPos(2, 5).Equals(new CellPos(2, 5)));
        }

        [Fact]
        public void Equals_DifferentX_ReturnsFalse()
        {
            Assert.False(new CellPos(2, 5).Equals(new CellPos(3, 5)));
        }

        [Fact]
        public void Equals_DifferentY_ReturnsFalse()
        {
            Assert.False(new CellPos(2, 5).Equals(new CellPos(2, 6)));
        }

        [Fact]
        public void Equals_SwappedCoordinates_ReturnsFalse()
        {
            Assert.False(new CellPos(2, 5).Equals(new CellPos(5, 2)));
        }

        [Fact]
        public void Equals_DefaultAndOrigin_ReturnsTrue()
        {
            Assert.True(default(CellPos).Equals(new CellPos(0, 0)));
        }

        [Fact]
        public void EqualsObject_BoxedSameCoordinates_ReturnsTrue()
        {
            object boxed = new CellPos(2, 5);

            Assert.True(new CellPos(2, 5).Equals(boxed));
        }

        [Fact]
        public void EqualsObject_BoxedDifferentCoordinates_ReturnsFalse()
        {
            object boxed = new CellPos(5, 2);

            Assert.False(new CellPos(2, 5).Equals(boxed));
        }

        [Fact]
        public void EqualsObject_Null_ReturnsFalse()
        {
            Assert.False(new CellPos(2, 5).Equals((object)null));
        }

        [Fact]
        public void EqualsObject_OtherType_ReturnsFalse()
        {
            Assert.False(new CellPos(2, 5).Equals("(2, 5)"));
            Assert.False(new CellPos(0, 0).Equals(0));
        }

        [Theory]
        [InlineData(0, 0)]
        [InlineData(3, 4)]
        [InlineData(-7, 9)]
        [InlineData(15, 15)]
        public void GetHashCode_EqualPositions_HaveEqualHashCodes(int x, int y)
        {
            Assert.Equal(new CellPos(x, y).GetHashCode(), new CellPos(x, y).GetHashCode());
        }

        [Fact]
        public void GetHashCode_DefaultAndOrigin_AreEqual()
        {
            Assert.Equal(default(CellPos).GetHashCode(), new CellPos(0, 0).GetHashCode());
        }

        [Fact]
        public void EqualityOperators_SameCoordinates_EqualAndNotUnequal()
        {
            var a = new CellPos(1, 2);
            var b = new CellPos(1, 2);

            Assert.True(a == b);
            Assert.False(a != b);
        }

        [Fact]
        public void EqualityOperators_DifferentCoordinates_UnequalAndNotEqual()
        {
            var a = new CellPos(1, 2);

            Assert.True(a != new CellPos(2, 1));
            Assert.False(a == new CellPos(2, 1));
            Assert.True(a != new CellPos(1, 3));
            Assert.True(a != new CellPos(0, 2));
        }

        [Fact]
        public void HashSet_EqualPositionsCollapseAndDifferentOnesStay()
        {
            var set = new HashSet<CellPos>
            {
                new CellPos(1, 2),
                new CellPos(1, 2),
                new CellPos(2, 1),
                new CellPos(0, 0),
                default
            };

            Assert.Equal(3, set.Count);
            Assert.Contains(new CellPos(2, 1), set);
            Assert.DoesNotContain(new CellPos(2, 2), set);
        }

        [Theory]
        [InlineData(3, 4, "(3, 4)")]
        [InlineData(0, 0, "(0, 0)")]
        [InlineData(-1, 15, "(-1, 15)")]
        [InlineData(15, 0, "(15, 0)")]
        public void ToString_Position_UsesParenthesesAndCommaSpace(int x, int y, string expected)
        {
            Assert.Equal(expected, new CellPos(x, y).ToString());
        }
    }
}
