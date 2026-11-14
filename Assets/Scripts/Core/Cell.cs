using System;

namespace BlastGame.Core
{
    /// <summary>Value type describing what a grid cell holds.</summary>
    public readonly struct Cell : IEquatable<Cell>
    {
        public static readonly Cell Empty;

        private readonly CellKind _kind;
        private readonly TileColor _color;

        private Cell(CellKind kind, TileColor color)
        {
            _kind = kind;
            _color = color;
        }

        public static Cell OfColor(TileColor color)
        {
            if (!Enum.IsDefined(typeof(TileColor), color))
                throw new ArgumentOutOfRangeException(nameof(color), "Undefined tile color: " + (int)color + ".");
            return new Cell(CellKind.Color, color);
        }

        public CellKind Kind
        {
            get { return _kind; }
        }

        public bool IsEmpty
        {
            get { return _kind == CellKind.Empty; }
        }

        public TileColor Color
        {
            get
            {
                if (_kind != CellKind.Color)
                    throw new InvalidOperationException("Only a colored cell has a color; this cell is " + _kind + ".");
                return _color;
            }
        }

        public bool Equals(Cell other)
        {
            if (_kind != other._kind) return false;
            return _kind != CellKind.Color || _color == other._color;
        }

        public override bool Equals(object obj)
        {
            return obj is Cell other && Equals(other);
        }

        public override int GetHashCode()
        {
            return _kind == CellKind.Color ? 1 + (int)_color * 31 : 0;
        }

        public override string ToString()
        {
            return _kind == CellKind.Color ? "Color(" + _color + ")" : "Empty";
        }

        public static bool operator ==(Cell a, Cell b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(Cell a, Cell b)
        {
            return !a.Equals(b);
        }
    }
}
