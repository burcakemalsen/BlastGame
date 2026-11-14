using System;

namespace BlastGame.Core
{
    /// <summary>One hit on a Pinata or Crate, with the cell before and after.</summary>
    public readonly struct ObstacleHit : IEquatable<ObstacleHit>
    {
        public ObstacleHit(CellPos pos, Cell before, Cell after)
        {
            throw new NotImplementedException();
        }

        public CellPos Pos
        {
            get { throw new NotImplementedException(); }
        }

        /// <summary>A Pinata or a Crate.</summary>
        public Cell Before
        {
            get { throw new NotImplementedException(); }
        }

        /// <summary>Cell.Empty (broken) or a Crate with one layer less.</summary>
        public Cell After
        {
            get { throw new NotImplementedException(); }
        }

        public bool Equals(ObstacleHit other)
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

        public static bool operator ==(ObstacleHit a, ObstacleHit b)
        {
            throw new NotImplementedException();
        }

        public static bool operator !=(ObstacleHit a, ObstacleHit b)
        {
            throw new NotImplementedException();
        }
    }
}
