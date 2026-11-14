using BlastGame.Core;
using UnityEngine;

namespace BlastGame.View
{
    /// <summary>Maps rules-layer colors to display colors.</summary>
    public static class TileColors
    {
        public static readonly Color Background = Hex(0x1E, 0x1E, 0x2E);

        // Special items without a color: neutral light and dark tones, unlike any tile color.
        public static readonly Color RocketBody = Hex(0xE8, 0xE8, 0xF0);
        public static readonly Color RocketArrows = Hex(0xB8, 0xBC, 0xCC);
        public static readonly Color BombBody = Hex(0x3A, 0x3C, 0x4A);
        public static readonly Color BombRing = Hex(0xD6, 0xD8, 0xE2);
        public static readonly Color OrbRing = new Color(1f, 1f, 1f, 0.85f);
        public static readonly Color Flash = Color.white;
        public static readonly Color Wave = Hex(0xFF, 0xE8, 0xC0);

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
