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
            var cp = new ContentPath("doc");
            var lineage = cp.Lineage.ToArray();
            var expect = new[]
            {
                new ContentPath(),
                new ContentPath("doc")
            };

            Assert.True(lineage.SequenceEqual(expect));
        }

        [Fact]
        public void Lineage2()
        {
            var cp = new ContentPath();
            var lineage = cp.Lineage.ToArray();
            var expect = new[]
            {
                new ContentPath()
            };

            Assert.True(lineage.SequenceEqual(expect));
        }

        [Fact]
        public void Parent()
        {
            var cp = new ContentPath("Readme.md");
            Assert.Equal(new ContentPath(), cp.Parent);
        }

        [Fact]
        public void Parent2()
        {
            var cp = new ContentPath();
            Assert.False(cp.Parent.HasValue);
        }

        [Fact]
        public void Name()
        {
            Assert.False(new ContentPath().Name.HasValue);
            Assert.Equal("c", new ContentPath("a", "b", "c").Name);
        }

        [Fact]
        public void IsAncestorOrEqual()
        {
            var dirA = new ContentPath("a");
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
