using Functional.Option;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace gitter
{
    public interface IHistory
    {
        /// <summary>
        /// Returns a list of recent changes as Markdown
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        Task<Option<string>> GetRecentChanges(ContentPath path);
    }
}
