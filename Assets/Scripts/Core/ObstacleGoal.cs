using System;

namespace BlastGame.Core
{
    /// <summary>Goal: break or collect <see cref="Count"/> obstacles of one kind.</summary>
    public readonly struct ObstacleGoal : IEquatable<ObstacleGoal>
    {
        private readonly ObstacleKind _kind;
        private readonly int _count;

        /// <summary>The count is not checked here (like ColorGoal's callers, GameSession and LevelData check it).</summary>
        public ObstacleGoal(ObstacleKind kind, int count)
        {
            if (!Enum.IsDefined(typeof(ObstacleKind), kind))
                throw new ArgumentOutOfRangeException(nameof(kind), "Undefined obstacle kind: " + (int)kind + ".");
            _kind = kind;
            _count = count;
        }

        public ObstacleKind Kind
        {
            get { return _kind; }
        }

        public int Count
        {
            get { return _count; }
        }

        public bool Equals(ObstacleGoal other)
        {
            return _kind == other._kind && _count == other._count;
        }

        public override bool Equals(object obj)
        {
            return obj is ObstacleGoal other && Equals(other);
        }

        public override int GetHashCode()
        {
            return unchecked((int)_kind * 397 ^ _count);
        }

        /// <summary>Formats as "Crate x3".</summary>
        public override string ToString()
        {
            return _kind + " x" + _count;
        }

        public static bool operator ==(ObstacleGoal a, ObstacleGoal b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(ObstacleGoal a, ObstacleGoal b)
        {
            return !a.Equals(b);
        }
    }
}
