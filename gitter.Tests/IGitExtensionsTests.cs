using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Xunit;

namespace gitter.Tests
{
    public class IGitExtensionsTests
    {
        [Fact]
        public void ReadLog()
        {
            using (var reader = new StreamReader(@"C:\temp\log"))
            {
                var c = IGitExtensions.ReadChanges(reader);

                var byName = c.GroupBy(_ => _.Commit.author.Email)
                    .Select(_ => new { user = _.Key, added = _.Sum(_c => _c.Stats.AddedLines) })
                    .ToList();
            }
        }
    }
}
