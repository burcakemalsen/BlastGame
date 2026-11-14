using System;

namespace BlastGame.Core
{
    /// <summary>Value type describing what a grid cell holds.</summary>
    public readonly struct Cell : IEquatable<Cell>
    {
        public static readonly Cell Empty;

        private readonly CellKind _kind;
        private readonly TileColor _color;

        public static Cell OfColor(TileColor color)
        {
            throw new NotImplementedException();
        }

        public CellKind Kind
        {
            get { throw new NotImplementedException(); }
        }

        public bool IsEmpty
        {
            get { throw new NotImplementedException(); }
        }

        public TileColor Color
        {
            get { throw new NotImplementedException(); }
        }

        public bool Equals(Cell other)
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

        public static bool operator ==(Cell a, Cell b)
        {
            throw new NotImplementedException();
        }

        public static bool operator !=(Cell a, Cell b)
        {
            throw new NotImplementedException();
        }
    }
}
