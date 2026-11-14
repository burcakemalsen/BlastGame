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
            _from = from;
            _to = to;
        }

        public CellPos From
        {
            get { return _from; }
        }

        public CellPos To
        {
            get { return _to; }
        }

        public bool Equals(CellMove other)
        {
            return _from == other._from && _to == other._to;
        }

        public override bool Equals(object obj)
        {
            return obj is CellMove other && Equals(other);
        }

        public override int GetHashCode()
        {
            return unchecked(_from.GetHashCode() * 397 ^ _to.GetHashCode());
        }

        /// <summary>Formats as "(x, y)->(x, y)".</summary>
        public override string ToString()
        {
            return _from + "->" + _to;
        }

        public static bool operator ==(CellMove a, CellMove b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(CellMove a, CellMove b)
        {
            return !a.Equals(b);
        }
    }
}
