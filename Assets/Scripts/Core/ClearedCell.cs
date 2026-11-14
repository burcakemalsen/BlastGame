using System;

namespace BlastGame.Core
{
    /// <summary>A cell emptied by an effect, with what it held before.</summary>
    public readonly struct ClearedCell : IEquatable<ClearedCell>
    {
        public ClearedCell(CellPos pos, Cell cell)
        {
            throw new NotImplementedException();
        }

        public CellPos Pos
        {
            get { throw new NotImplementedException(); }
        }

        public Cell Cell
        {
            get { throw new NotImplementedException(); }
        }

        public bool Equals(ClearedCell other)
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

        /// <summary>Format: "(x, y) Color(Red)".</summary>
        public override string ToString()
        {
            throw new NotImplementedException();
        }

        public static bool operator ==(ClearedCell a, ClearedCell b)
        {
            throw new NotImplementedException();
        }

        public static bool operator !=(ClearedCell a, ClearedCell b)
        {
            throw new NotImplementedException();
        }
    }
}
