using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace gitter.Models
{
    public class ContentModel
    {
        public ContentModel(ContentPath path, string title, string body)
        {
            Path = path;
            this.Title = title;
            this.Body = body;
        }

        public ContentPath Path { get; }

        public string Body { get; }

        public string Title { get; }
    }

    public class RawContentModel
    {
        public RawContentModel(ContentPath path, Stream data, string mimeType)
        {
            Path = path;
            Data = data;
            MimeType = mimeType;
        }

        public ContentPath Path { get; }
        public Stream Data { get; }
        public string MimeType { get; }
    }
}
