using System;

namespace BlastGame.Core
{
    /// <summary>One obstacle placed by a level file.</summary>
    public readonly struct ObstaclePlacement : IEquatable<ObstaclePlacement>
    {
        public ObstaclePlacement(ObstacleKind kind, CellPos pos, int layers)
        {
            throw new NotImplementedException();
        }

        public ObstacleKind Kind
        {
            get { throw new NotImplementedException(); }
        }

        public CellPos Pos
        {
            get { throw new NotImplementedException(); }
        }

        public int Layers
        {
            get { throw new NotImplementedException(); }
        }

        public bool Equals(ObstaclePlacement other)
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

        public static bool operator ==(ObstaclePlacement a, ObstaclePlacement b)
        {
            throw new NotImplementedException();
        }

        public static bool operator !=(ObstaclePlacement a, ObstaclePlacement b)
        {
            throw new NotImplementedException();
        }
    }
}
