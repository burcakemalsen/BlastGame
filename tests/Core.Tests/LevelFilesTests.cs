using System;
using System.IO;
using System.Linq;
using Xunit;

namespace BlastGame.Core.Tests
{
    public class LevelFilesTests
    {
        private static string[] LevelPaths()
        {
            string dir = AppContext.BaseDirectory;
            while (dir != null && !Directory.Exists(Path.Combine(dir, "Assets")))
            {
                dir = Path.GetDirectoryName(dir.TrimEnd(Path.DirectorySeparatorChar));
            }

            Assert.True(dir != null, "Repo root (a folder containing 'Assets') was not found above the test folder.");

            string levels = Path.Combine(dir, "Assets", "Resources", "Levels");
            Assert.True(Directory.Exists(levels), "Folder not found: " + levels);

            return Directory.GetFiles(levels, "*.json").OrderBy(p => p, StringComparer.Ordinal).ToArray();
        }

        [Fact]
        public void LevelFolder_HasAtLeastThreeFiles()
        {
            Assert.True(LevelPaths().Length >= 3);
        }

        [Fact]
        public void LevelFiles_EachParsesWithoutError()
        {
            foreach (string path in LevelPaths())
            {
                var ex = Record.Exception(() => LevelParser.Parse(File.ReadAllText(path)));
                Assert.True(ex == null, Path.GetFileName(path) + ": " + ex?.Message);
            }
        }
    }
}
