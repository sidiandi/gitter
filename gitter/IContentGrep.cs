using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace gitter
{
    public class GrepResult
    {
        public ContentPath Path;
        public int LineNumber;
        public string Text;
    }

    public interface IContentGrep
    {
        Task<IEnumerable<GrepResult>> Grep(string q);
    }
}
