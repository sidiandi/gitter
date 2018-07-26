using System;
using System.Linq;
using Xunit;

namespace gitter.Tests
{
    public class ContentPathTests
    {
        [Fact]
        public void Lineage()
        {
            var cp = new ContentPath(new[] { "doc", ContentPath.DirectoryContent });
            var lineage = cp.Lineage.ToArray();
            var expect = new[]
            {
                new ContentPath(new[]{ContentPath.DirectoryContent }),
                new ContentPath(new[] { "doc", ContentPath.DirectoryContent })
            };

            Assert.True(lineage.SequenceEqual(expect));
        }

        [Fact]
        public void Name()
        {
            Assert.False(new ContentPath(new string [] { }).NameWithDirSlash.HasValue);
            Assert.True(new ContentPath(new string[] { }).AsDirectory.NameWithDirSlash.HasValue);
            Assert.Equal("$/", new ContentPath(new string[] { }).AsDirectory.NameWithDirSlash);
        }

        [Fact]
        public void GetRelativePath()
        {
            var dirA = new ContentPath(new[] { "a" }).AsDirectory;
            var itemB = dirA.CatDir("b");
            Assert.Equal(
                new ContentPath(new[] { "b" }), 
                dirA.GetRelativePath(itemB));

            Assert.Equal(
                new ContentPath(new[] { "." }),
                itemB.GetRelativePath(dirA));
        }

        [Fact]
        public void GetRelativePath3()
        {
            var doc = new ContentPath(new[] { "doc" }).AsDirectory;
            var existing = doc.CatDir("existing").AsDirectory;
            var r = doc.GetRelativePath(existing);
            Assert.Equal(new ContentPath(new[] { "existing" }).AsDirectory, r);
        }

        [Fact]
        public void GetRelativePath2()
        {
            var dirA = new ContentPath(new[] { "a" }).AsDirectory;
            var dirB = new ContentPath(new[] { "b" }).AsDirectory;
            Assert.Equal(
                new ContentPath(new[] { "..", "b" }).AsDirectory,
                dirA.GetRelativePath(dirB));
        }

        [Fact]
        public void IsAncestorOrEqual()
        {
            var dirA = new ContentPath(new[] { "a" }).AsDirectory;
            var itemB = dirA.CatDir("b");
            Assert.True(dirA.IsAncestorOrSelf(itemB));
            Assert.True(dirA.IsAncestorOrSelf(dirA));
            Assert.False(itemB.IsAncestorOrSelf(dirA));
        }

        [Fact]
        public void CatDir()
        {
            Assert.Equal(new ContentPath(new[] { "a", "b" }),
                new ContentPath(new[] { "a" }).CatDir("b"));
        }
    }
}
