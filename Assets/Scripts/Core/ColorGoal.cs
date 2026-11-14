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
            throw new NotImplementedException();
        }

        public TileColor Color
        {
            get { throw new NotImplementedException(); }
        }

        public int Count
        {
            get { throw new NotImplementedException(); }
        }

        public bool Equals(ColorGoal other)
        {
            throw new NotImplementedException();
        }

        public override bool Equals(object obj)
        {
            throw new NotImplementedException();
        }

        public override int GetHashCode()
        {
            throw new NotImplementedException();
        }

        /// <summary>Formats as "Red x20".</summary>
        public override string ToString()
        {
            throw new NotImplementedException();
        }

        public static bool operator ==(ColorGoal a, ColorGoal b)
        {
            throw new NotImplementedException();
        }

        public static bool operator !=(ColorGoal a, ColorGoal b)
        {
            throw new NotImplementedException();
        }
    }
}
