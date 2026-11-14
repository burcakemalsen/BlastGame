using System;

namespace BlastGame.Core
{
    /// <summary>Thrown when a level file is malformed or breaks a level rule.</summary>
    public sealed class LevelFormatException : Exception
    {
        public LevelFormatException(string message) : base(message)
        {
        }

        public LevelFormatException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
