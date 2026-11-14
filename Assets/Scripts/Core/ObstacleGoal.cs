using System;

namespace BlastGame.Core
{
    /// <summary>Goal: break or collect <see cref="Count"/> obstacles of one kind.</summary>
    public readonly struct ObstacleGoal : IEquatable<ObstacleGoal>
    {
        public ObstacleGoal(ObstacleKind kind, int count)
        {
            throw new NotImplementedException();
        }

        public ObstacleKind Kind
        {
            get { throw new NotImplementedException(); }
        }

        public int Count
        {
            get { throw new NotImplementedException(); }
        }

        public bool Equals(ObstacleGoal other)
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

        public override string ToString()
        {
            throw new NotImplementedException();
        }

        public static bool operator ==(ObstacleGoal a, ObstacleGoal b)
        {
            throw new NotImplementedException();
        }

        public static bool operator !=(ObstacleGoal a, ObstacleGoal b)
        {
            throw new NotImplementedException();
        }
    }
}
