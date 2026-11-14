namespace BlastGame.Core
{
    /// <summary>The only source of randomness for the rules layer.</summary>
    public interface IRandomSource
    {
        /// <summary>Returns a value in [0, maxExclusive).</summary>
        int Next(int maxExclusive);
    }
}
