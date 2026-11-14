using System;

namespace BlastGame.Core
{
    /// <summary>A cell emptied by an effect, with what it held before.</summary>
    public readonly struct ClearedCell : IEquatable<ClearedCell>
    {
        private readonly CellPos _pos;
        private readonly Cell _cell;

        public ClearedCell(CellPos pos, Cell cell)
        {
            _pos = pos;
            _cell = cell;
        }

        public CellPos Pos
        {
            get { return _pos; }
        }

        public Cell Cell
        {
            get { return _cell; }
        }

        public bool Equals(ClearedCell other)
        {
            return _pos == other._pos && _cell == other._cell;
        }

        public override bool Equals(object obj)
        {
            return obj is ClearedCell other && Equals(other);
        }

        public override int GetHashCode()
        {
            return unchecked(_pos.GetHashCode() * 397 ^ _cell.GetHashCode());
        }

        /// <summary>Format: "(x, y) Color(Red)".</summary>
        public override string ToString()
        {
            return _pos + " " + _cell;
        }

        public static bool operator ==(ClearedCell a, ClearedCell b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(ClearedCell a, ClearedCell b)
        {
            return !a.Equals(b);
        }
    }
}
