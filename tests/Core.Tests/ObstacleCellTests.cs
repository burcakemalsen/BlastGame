using System;
using Xunit;

namespace BlastGame.Core.Tests
{
    /// <summary>Spec 07, Cell: obstacle factory, accessors, balloon flag, equality.</summary>
    public class ObstacleCellTests
    {
        private static Cell Red => Cell.OfColor(TileColor.Red);

        [Fact]
        public void OfObstacle_Pinata_IsObstacleWithOneLayer()
        {
            Cell cell = Cell.OfObstacle(ObstacleKind.Pinata, 1);

            Assert.Equal(CellKind.Obstacle, cell.Kind);
            Assert.True(cell.IsObstacle);
            Assert.False(cell.IsEmpty);
            Assert.False(cell.IsSpecial);
            Assert.False(cell.HasBalloon);
            Assert.Equal(ObstacleKind.Pinata, cell.Obstacle);
            Assert.Equal(1, cell.Layers);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(3)]
        public void OfObstacle_CrateLayersInRange_KeepsLayers(int layers)
        {
            Cell cell = Cell.OfObstacle(ObstacleKind.Crate, layers);

            Assert.Equal(ObstacleKind.Crate, cell.Obstacle);
            Assert.Equal(layers, cell.Layers);
        }

        [Fact]
        public void OfObstacle_Anchor_IsObstacle()
        {
            Cell cell = Cell.OfObstacle(ObstacleKind.Anchor, 1);

            Assert.True(cell.IsObstacle);
            Assert.Equal(ObstacleKind.Anchor, cell.Obstacle);
        }

        [Theory]
        [InlineData(ObstacleKind.Crate, 0)]
        [InlineData(ObstacleKind.Crate, 4)]
        [InlineData(ObstacleKind.Pinata, 2)]
        [InlineData(ObstacleKind.Anchor, 0)]
        public void OfObstacle_LayersOutOfRange_ThrowsArgumentOutOfRange(ObstacleKind kind, int layers)
        {
            var ex = Assert.Throws<ArgumentOutOfRangeException>(() => Cell.OfObstacle(kind, layers));
            Assert.Equal("layers", ex.ParamName);
        }

        [Fact]
        public void OfObstacle_UndefinedKind_ThrowsArgumentOutOfRange()
        {
            var ex = Assert.Throws<ArgumentOutOfRangeException>(() => Cell.OfObstacle((ObstacleKind)99, 1));
            Assert.Equal("kind", ex.ParamName);
        }

        [Fact]
        public void OfObstacle_WaterBalloon_ThrowsArgumentException()
        {
            var ex = Assert.Throws<ArgumentException>(() => Cell.OfObstacle(ObstacleKind.WaterBalloon, 1));
            Assert.Equal("kind", ex.ParamName);
        }

        public static TheoryData<Cell> NonObstacleCells => new TheoryData<Cell>
        {
            Cell.Empty,
            Cell.OfColor(TileColor.Red),
            Cell.OfSpecial(SpecialKind.Bomb),
        };

        [Theory]
        [MemberData(nameof(NonObstacleCells))]
        public void ObstacleAndLayers_NonObstacleCell_ThrowInvalidOperation(Cell cell)
        {
            Assert.False(cell.IsObstacle);
            Assert.Throws<InvalidOperationException>(() => cell.Obstacle);
            Assert.Throws<InvalidOperationException>(() => cell.Layers);
        }

        [Fact]
        public void ColorAndSpecial_ObstacleCell_ThrowInvalidOperation()
        {
            Cell cell = Cell.OfObstacle(ObstacleKind.Crate, 2);

            Assert.Throws<InvalidOperationException>(() => cell.Color);
            Assert.Throws<InvalidOperationException>(() => cell.Special);
        }

        [Fact]
        public void HasBalloon_EveryKindWithoutBalloon_IsFalse()
        {
            Assert.False(Cell.Empty.HasBalloon);
            Assert.False(Red.HasBalloon);
            Assert.False(Cell.OfSpecial(SpecialKind.RocketVertical).HasBalloon);
            Assert.False(Cell.OfOrb(TileColor.Blue).HasBalloon);
        }

        [Fact]
        public void WithBalloon_TrueOnColor_KeepsColorAndSetsFlag()
        {
            Cell cell = Cell.OfColor(TileColor.Blue).WithBalloon(true);

            Assert.Equal(CellKind.Color, cell.Kind);
            Assert.Equal(TileColor.Blue, cell.Color);
            Assert.True(cell.HasBalloon);
            Assert.False(cell.IsObstacle);
        }

        [Fact]
        public void WithBalloon_FalseOnBalloonColor_EqualsPlainColor()
        {
            Cell cell = Red.WithBalloon(true).WithBalloon(false);

            Assert.False(cell.HasBalloon);
            Assert.True(cell == Red);
        }

        public static TheoryData<Cell, bool> NonColorCells => new TheoryData<Cell, bool>
        {
            { Cell.Empty, true },
            { Cell.OfSpecial(SpecialKind.Bomb), false },
            { Cell.OfOrb(TileColor.Red), true },
            { Cell.OfObstacle(ObstacleKind.Pinata, 1), false },
        };

        [Theory]
        [MemberData(nameof(NonColorCells))]
        public void WithBalloon_NonColorCell_ThrowsInvalidOperation(Cell cell, bool value)
        {
            Assert.Throws<InvalidOperationException>(() => cell.WithBalloon(value));
        }

        [Fact]
        public void Equality_BalloonFlag_Differs()
        {
            Cell plain = Red;
            Cell balloon = Red.WithBalloon(true);

            Assert.False(plain == balloon);
            Assert.True(plain != balloon);
            Assert.False(plain.Equals(balloon));
            Assert.True(balloon == Cell.OfColor(TileColor.Red).WithBalloon(true));
            Assert.Equal(balloon.GetHashCode(), Cell.OfColor(TileColor.Red).WithBalloon(true).GetHashCode());
        }

        [Fact]
        public void Equality_CrateLayers_Differ()
        {
            Cell c2 = Cell.OfObstacle(ObstacleKind.Crate, 2);
            Cell c3 = Cell.OfObstacle(ObstacleKind.Crate, 3);

            Assert.True(c2 != c3);
            Assert.False(c2.Equals((object)c3));
            Assert.True(c2 == Cell.OfObstacle(ObstacleKind.Crate, 2));
            Assert.Equal(c2.GetHashCode(), Cell.OfObstacle(ObstacleKind.Crate, 2).GetHashCode());
        }

        [Fact]
        public void Equality_ObstacleKinds_DifferFromEachOtherAndFromOtherCells()
        {
            Cell pinata = Cell.OfObstacle(ObstacleKind.Pinata, 1);
            Cell anchor = Cell.OfObstacle(ObstacleKind.Anchor, 1);
            Cell crate1 = Cell.OfObstacle(ObstacleKind.Crate, 1);

            Assert.True(pinata != anchor);
            Assert.True(pinata != crate1);
            Assert.True(pinata != Cell.Empty);
            Assert.True(pinata != Red);
            Assert.True(pinata != Cell.OfSpecial(SpecialKind.RocketHorizontal));
        }

        [Fact]
        public void ToString_ObstacleAndBalloon_NonEmptyAndDistinct()
        {
            string c2 = Cell.OfObstacle(ObstacleKind.Crate, 2).ToString();
            string c3 = Cell.OfObstacle(ObstacleKind.Crate, 3).ToString();
            string balloon = Red.WithBalloon(true).ToString();

            Assert.False(string.IsNullOrEmpty(c2));
            Assert.NotEqual(c2, c3);
            Assert.NotEqual(Red.ToString(), balloon);
        }
    }
}
