using Xunit;

namespace BlastGame.Core.Tests
{
    public class CellMoveTests
    {
        private static CellMove M(int fx, int fy, int tx, int ty)
        {
            return new CellMove(new CellPos(fx, fy), new CellPos(tx, ty));
        }

        [Fact]
        public void Constructor_FromAndTo_StoresBoth()
        {
            CellMove move = M(3, 7, 3, 2);
            Assert.Equal(new CellPos(3, 7), move.From);
            Assert.Equal(new CellPos(3, 2), move.To);
        }

        [Fact]
        public void Equals_SameFromAndTo_TrueByAllForms()
        {
            CellMove a = M(1, 4, 1, 0);
            CellMove b = M(1, 4, 1, 0);
            Assert.True(a.Equals(b));
            Assert.True(a.Equals((object)b));
            Assert.True(a == b);
            Assert.False(a != b);
        }

        [Theory]
        [InlineData(2, 4, 1, 0)]
        [InlineData(1, 5, 1, 0)]
        [InlineData(1, 4, 2, 0)]
        [InlineData(1, 4, 1, 1)]
        public void Equals_OneCoordinateDiffers_FalseByAllForms(int fx, int fy, int tx, int ty)
        {
            CellMove a = M(1, 4, 1, 0);
            CellMove b = M(fx, fy, tx, ty);
            Assert.False(a.Equals(b));
            Assert.False(a.Equals((object)b));
            Assert.False(a == b);
            Assert.True(a != b);
        }

        [Fact]
        public void Equals_FromAndToSwapped_False()
        {
            Assert.False(M(0, 3, 0, 1).Equals(M(0, 1, 0, 3)));
        }

        [Fact]
        public void EqualsObject_NullOrOtherType_False()
        {
            CellMove a = M(0, 1, 0, 0);
            Assert.False(a.Equals(null));
            Assert.False(a.Equals((object)new CellPos(0, 1)));
            Assert.False(a.Equals((object)"(0, 1)->(0, 0)"));
        }

        [Fact]
        public void GetHashCode_EqualMoves_SameHash()
        {
            Assert.Equal(M(15, 15, 15, 0).GetHashCode(), M(15, 15, 15, 0).GetHashCode());
            Assert.Equal(M(0, 0, 0, 0).GetHashCode(), default(CellMove).GetHashCode());
        }

        [Fact]
        public void GetHashCode_SameMove_StableAcrossCalls()
        {
            CellMove a = M(2, 9, 2, 3);
            Assert.Equal(a.GetHashCode(), a.GetHashCode());
        }

        [Theory]
        [InlineData(0, 0, 0, 0, "(0, 0)->(0, 0)")]
        [InlineData(3, 7, 3, 2, "(3, 7)->(3, 2)")]
        [InlineData(15, 15, 15, 0, "(15, 15)->(15, 0)")]
        public void ToString_Move_UsesArrowBetweenPositions(int fx, int fy, int tx, int ty, string expected)
        {
            Assert.Equal(expected, M(fx, fy, tx, ty).ToString());
        }
    }
}
