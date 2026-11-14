using BlastGame.Core;
using UnityEngine;

namespace BlastGame.View
{
    /// <summary>Maps rules-layer colors to display colors.</summary>
    public static class TileColors
    {
        public static readonly Color Background = Hex(0x1E, 0x1E, 0x2E);

        public static Color ToColor(TileColor color)
        {
            switch (color)
            {
                case TileColor.Red: return Hex(0xE7, 0x4C, 0x3C);
                case TileColor.Green: return Hex(0x2E, 0xCC, 0x71);
                case TileColor.Blue: return Hex(0x34, 0x98, 0xDB);
                case TileColor.Yellow: return Hex(0xF1, 0xC4, 0x0F);
                case TileColor.Purple: return Hex(0x9B, 0x59, 0xB6);
                case TileColor.Orange: return Hex(0xE6, 0x7E, 0x22);
                default: return Color.white;
            }
        }

        private static Color Hex(byte r, byte g, byte b)
        {
            return new Color32(r, g, b, 255);
        }
    }
}
