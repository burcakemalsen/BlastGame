using System;
using Xunit;

namespace BlastGame.Core.Tests
{
    public class ColorGoalTests
    {
        [Fact]
        public void Ctor_ValidArguments_StoresColorAndCount()
        {
            var goal = new ColorGoal(TileColor.Blue, 7);

            Assert.Equal(TileColor.Blue, goal.Color);
            Assert.Equal(7, goal.Count);
        }

        [Fact]
        public void Ctor_CountOne_IsAccepted()
        {
            var goal = new ColorGoal(TileColor.Red, 1);

            Assert.Equal(1, goal.Count);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(int.MinValue)]
        public void Ctor_CountBelowOne_ThrowsArgumentOutOfRangeWithParamName(int count)
        {
            var ex = Assert.Throws<ArgumentOutOfRangeException>(() => new ColorGoal(TileColor.Red, count));

            Assert.Equal("count", ex.ParamName);
        }

        [Fact]
        public void Equals_SameColorAndCount_IsTrue()
        {
            var a = new ColorGoal(TileColor.Green, 5);
            var b = new ColorGoal(TileColor.Green, 5);

            Assert.True(a.Equals(b));
            Assert.True(a == b);
            Assert.False(a != b);
        }

        [Fact]
        public void Equals_DifferentColor_IsFalse()
        {
            var a = new ColorGoal(TileColor.Green, 5);
            var b = new ColorGoal(TileColor.Blue, 5);

            Assert.False(a.Equals(b));
            Assert.False(a == b);
            Assert.True(a != b);
        }

        [Fact]
        public void Equals_DifferentCount_IsFalse()
        {
            var a = new ColorGoal(TileColor.Green, 5);
            var b = new ColorGoal(TileColor.Green, 6);

            Assert.False(a.Equals(b));
            Assert.False(a == b);
            Assert.True(a != b);
        }

        [Fact]
        public void EqualsObject_BoxedEqualGoal_IsTrue()
        {
            object boxed = new ColorGoal(TileColor.Red, 2);

            Assert.True(new ColorGoal(TileColor.Red, 2).Equals(boxed));
        }

        [Fact]
        public void EqualsObject_NullOrOtherType_IsFalse()
        {
            var goal = new ColorGoal(TileColor.Red, 2);

            Assert.False(goal.Equals((object)null));
            Assert.False(goal.Equals("Red x2"));
            Assert.False(goal.Equals((object)TileColor.Red));
        }

        [Fact]
        public void GetHashCode_EqualGoals_HaveSameHash()
        {
            var a = new ColorGoal(TileColor.Yellow, 9);
            var b = new ColorGoal(TileColor.Yellow, 9);

            Assert.Equal(a.GetHashCode(), b.GetHashCode());
        }

        [Fact]
        public void GetHashCode_EqualGoals_WorkAsSameDictionaryKey()
        {
            var set = new System.Collections.Generic.HashSet<ColorGoal>
            {
                new ColorGoal(TileColor.Purple, 3),
                new ColorGoal(TileColor.Purple, 3),
                new ColorGoal(TileColor.Purple, 4)
            };

            Assert.Equal(2, set.Count);
        }

        [Theory]
        [InlineData(TileColor.Red, 20, "Red x20")]
        [InlineData(TileColor.Blue, 1, "Blue x1")]
        [InlineData(TileColor.Orange, 105, "Orange x105")]
        public void ToString_FormatsColorAndCount(TileColor color, int count, string expected)
        {
            Assert.Equal(expected, new ColorGoal(color, count).ToString());
        }
    }
}
