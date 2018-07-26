using Functional.Option;
using gitter.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace gitter
{
    public interface IContentProvider
    {
        Option<Stream> Read(ContentPath path);
        IEnumerable<ContentPath> GetChildren(ContentPath path);
        bool Exists(ContentPath mdFile);
    }
}
