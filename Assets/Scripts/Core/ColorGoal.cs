using System;

namespace BlastGame.Core
{
    /// <summary>Goal: blast <see cref="Count"/> tiles of one color.</summary>
    public readonly struct ColorGoal : IEquatable<ColorGoal>
    {
        private readonly TileColor _color;
        private readonly int _count;

        public ColorGoal(TileColor color, int count)
        {
            if (count < 1)
                throw new ArgumentOutOfRangeException(nameof(count), "A goal needs at least 1 tile.");
            _color = color;
            _count = count;
        }

        public TileColor Color
        {
            get { return _color; }
        }

        public int Count
        {
            get { return _count; }
        }

        public bool Equals(ColorGoal other)
        {
            return _color == other._color && _count == other._count;
        }

        public override bool Equals(object obj)
        {
            return obj is ColorGoal other && Equals(other);
        }

        public override int GetHashCode()
        {
            return unchecked((int)_color * 397 ^ _count);
        }

        /// <summary>Formats as "Red x20".</summary>
        public override string ToString()
        {
            return _color + " x" + _count;
        }

        public static bool operator ==(ColorGoal a, ColorGoal b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(ColorGoal a, ColorGoal b)
        {
            return !a.Equals(b);
        }
    }
}
