using System;

namespace BlastGame.Core
{
    /// <summary>A cell coordinate; (0, 0) is bottom-left.</summary>
    public readonly struct CellPos : IEquatable<CellPos>
    {
        private readonly int _x;
        private readonly int _y;

        public CellPos(int x, int y)
        {
            _x = x;
            _y = y;
        }

        public int X
        {
            get { return _x; }
        }

        public int Y
        {
            get { return _y; }
        }

        public bool Equals(CellPos other)
        {
            return _x == other._x && _y == other._y;
        }

        public override bool Equals(object obj)
        {
            return obj is CellPos other && Equals(other);
        }

        public override int GetHashCode()
        {
            return unchecked(_x * 397 ^ _y);
        }

        public override string ToString()
        {
            return "(" + _x + ", " + _y + ")";
        }

        public static bool operator ==(CellPos a, CellPos b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(CellPos a, CellPos b)
        {
            return !a.Equals(b);
        }
    }
}
