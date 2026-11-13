using BlastGame.Core;
using Xunit;

namespace BlastGame.Core.Tests
{
    public class SmokeTests
    {
        [Fact]
        public void CoreAssembly_IsReachableFromTests()
        {
            Assert.Equal("BlastGame.Core", AssemblyMarker.Name);
        }
    }
}
