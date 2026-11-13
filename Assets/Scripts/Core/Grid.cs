namespace BlastGame.Core
{
    /// <summary>Game board model. (0, 0) is the bottom-left cell; y grows upward.</summary>
    public sealed class Grid
    {
        public const int MinSize = 1;
        public const int MaxSize = 16;

        public Grid(int width, int height)
        {
            throw new System.NotImplementedException();
        }

        public int Width
        {
            get { throw new System.NotImplementedException(); }
        }

        public int Height
        {
            get { throw new System.NotImplementedException(); }
        }

        public bool IsInside(int x, int y)
        {
            throw new System.NotImplementedException();
        }

        /// <summary>Returns the cell color; null means the cell is empty.</summary>
        public TileColor? Get(int x, int y)
        {
            throw new System.NotImplementedException();
        }

        public void Set(int x, int y, TileColor? color)
        {
            throw new System.NotImplementedException();
        }
    }
}
