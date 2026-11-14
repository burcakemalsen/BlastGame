using System;

namespace BlastGame.Core
{
    /// <summary>A tile moving from one cell to another (used by gravity).</summary>
    public readonly struct CellMove : IEquatable<CellMove>
    {
        private readonly CellPos _from;
        private readonly CellPos _to;

        public CellMove(CellPos from, CellPos to)
        {
            throw new System.NotImplementedException();
        }

        public CellPos From
        {
            get { throw new System.NotImplementedException(); }
        }

        public CellPos To
        {
            get { throw new System.NotImplementedException(); }
        }

        public bool Equals(CellMove other)
        {
            throw new System.NotImplementedException();
        }

        public override bool Equals(object obj)
        {
            throw new System.NotImplementedException();
        }

        public override int GetHashCode()
        {
            throw new System.NotImplementedException();
        }

        /// <summary>Formats as "(x, y)->(x, y)".</summary>
        public override string ToString()
        {
            throw new System.NotImplementedException();
        }

        public static bool operator ==(CellMove a, CellMove b)
        {
            throw new System.NotImplementedException();
        }

        public static bool operator !=(CellMove a, CellMove b)
        {
            throw new System.NotImplementedException();
        }
    }
}
