using System;

namespace BlastGame.Core
{
    /// <summary>One hit on a Pinata or Crate, with the cell before and after.</summary>
    public readonly struct ObstacleHit : IEquatable<ObstacleHit>
    {
        private readonly CellPos _pos;
        private readonly Cell _before;
        private readonly Cell _after;

        public ObstacleHit(CellPos pos, Cell before, Cell after)
        {
            _pos = pos;
            _before = before;
            _after = after;
        }

        public CellPos Pos
        {
            get { return _pos; }
        }

        /// <summary>A Pinata or a Crate.</summary>
        public Cell Before
        {
            get { return _before; }
        }

        /// <summary>Cell.Empty (broken) or a Crate with one layer less.</summary>
        public Cell After
        {
            get { return _after; }
        }

        public bool Equals(ObstacleHit other)
        {
            return _pos == other._pos && _before == other._before && _after == other._after;
        }

        public override bool Equals(object obj)
        {
            return obj is ObstacleHit other && Equals(other);
        }

        public override int GetHashCode()
        {
            return unchecked((_pos.GetHashCode() * 397 ^ _before.GetHashCode()) * 397 ^ _after.GetHashCode());
        }

        /// <summary>Format: "(x, y) Obstacle(Crate, 2) -> Obstacle(Crate, 1)".</summary>
        public override string ToString()
        {
            return _pos + " " + _before + " -> " + _after;
        }

        public static bool operator ==(ObstacleHit a, ObstacleHit b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(ObstacleHit a, ObstacleHit b)
        {
            return !a.Equals(b);
        }
    }
}
