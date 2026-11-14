using System;

namespace BlastGame.Core
{
    /// <summary>Value type describing what a grid cell holds.</summary>
    public readonly struct Cell : IEquatable<Cell>
    {
        public static readonly Cell Empty;

        private readonly CellKind _kind;
        private readonly TileColor _color;     // Color cells and Orbs
        private readonly SpecialKind _special; // Special cells only

        private Cell(CellKind kind, TileColor color, SpecialKind special)
        {
            _kind = kind;
            _color = color;
            _special = special;
        }

        public static Cell OfColor(TileColor color)
        {
            if (!Enum.IsDefined(typeof(TileColor), color))
                throw new ArgumentOutOfRangeException(nameof(color), "Undefined tile color: " + (int)color + ".");
            return new Cell(CellKind.Color, color, default(SpecialKind));
        }

        /// <summary>A Rocket or a Bomb. Use <see cref="OfOrb"/> for an Orb, which needs a color.</summary>
        public static Cell OfSpecial(SpecialKind kind)
        {
            if (!Enum.IsDefined(typeof(SpecialKind), kind))
                throw new ArgumentOutOfRangeException(nameof(kind), "Undefined special kind: " + (int)kind + ".");
            if (kind == SpecialKind.Orb)
                throw new ArgumentException("An Orb needs a color; use OfOrb.", nameof(kind));
            return new Cell(CellKind.Special, default(TileColor), kind);
        }

        public static Cell OfOrb(TileColor color)
        {
            if (!Enum.IsDefined(typeof(TileColor), color))
                throw new ArgumentOutOfRangeException(nameof(color), "Undefined tile color: " + (int)color + ".");
            return new Cell(CellKind.Special, color, SpecialKind.Orb);
        }

        public CellKind Kind
        {
            get { return _kind; }
        }

        public bool IsEmpty
        {
            get { return _kind == CellKind.Empty; }
        }

        public bool IsSpecial
        {
            get { return _kind == CellKind.Special; }
        }

        public SpecialKind Special
        {
            get
            {
                if (_kind != CellKind.Special)
                    throw new InvalidOperationException("Only a special cell has a special kind; this cell is " + _kind + ".");
                return _special;
            }
        }

        public TileColor OrbColor
        {
            get
            {
                if (_kind != CellKind.Special || _special != SpecialKind.Orb)
                    throw new InvalidOperationException("Only an Orb has an orb color; this cell is " + ToString() + ".");
                return _color;
            }
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

        private bool IsOrb
        {
            get { return _kind == CellKind.Special && _special == SpecialKind.Orb; }
        }

        public bool Equals(Cell other)
        {
            if (_kind != other._kind) return false;
            switch (_kind)
            {
                case CellKind.Color:
                    return _color == other._color;
                case CellKind.Special:
                    return _special == other._special && (_special != SpecialKind.Orb || _color == other._color);
                default:
                    return true;
            }
        }

        public override bool Equals(object obj)
        {
            return obj is Cell other && Equals(other);
        }

        public override int GetHashCode()
        {
            switch (_kind)
            {
                case CellKind.Color:
                    return 1 + (int)_color * 31;
                case CellKind.Special:
                    return 1000 + (int)_special * 31 + (IsOrb ? 1 + (int)_color : 0) * 7;
                default:
                    return 0;
            }
        }

        /// <summary>"Empty", "Color(Red)", "Special(Bomb)", "Special(Orb, Red)".</summary>
        public override string ToString()
        {
            switch (_kind)
            {
                case CellKind.Color:
                    return "Color(" + _color + ")";
                case CellKind.Special:
                    return IsOrb ? "Special(Orb, " + _color + ")" : "Special(" + _special + ")";
                default:
                    return "Empty";
            }
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
