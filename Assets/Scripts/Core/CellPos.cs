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
            throw new NotImplementedException();
        }

        public int X
        {
            get { throw new NotImplementedException(); }
        }

        public int Y
        {
            get { throw new NotImplementedException(); }
        }

        public bool Equals(CellPos other)
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

        public static bool operator ==(CellPos a, CellPos b)
        {
            throw new NotImplementedException();
        }

        public static bool operator !=(CellPos a, CellPos b)
        {
            throw new NotImplementedException();
        }
    }
}
