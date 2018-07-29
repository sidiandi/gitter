using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace gitter.Tests
{
    public class MarkdownExtensionsTests
    {
        [Fact]
        public void Table()
        {
            var items = Enumerable.Range(0, 10);
            var s = items.MarkdownTable(_ => _.ToString(), _ => (_ + 1).ToString());
            Console.WriteLine(s);
        }
    }
}
