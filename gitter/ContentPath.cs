using Functional.Option;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;

namespace gitter
{
    /// <summary>
    /// Enforces path $/name/name/name/.../name
    /// </summary>
    public class ContentPath
    {
        const string separator = "/";
        const string root = "$";

        public ContentPath(IEnumerable<string> parts)
        {
            this.parts = parts.ToArray();
        }

        string[] parts;

        public IEnumerable<string> Parts => parts;

        public override string ToString()
        {
            return String.Join(String.Empty, parts.Select(_ => separator + WebUtility.UrlEncode(_)));
        }

        internal static ContentPath FromUrlPath(string path)
        {
            if (String.IsNullOrEmpty(path))
            {
                return new ContentPath(new string[] { });
            }

            var cp = new ContentPath(path.Split(separator, StringSplitOptions.RemoveEmptyEntries)
                .Select(WebUtility.UrlDecode));
            return cp;
        }

        public string Href => String.Join(String.Empty, parts.Select(_ => separator + WebUtility.UrlEncode(_)));

        public string GetFileSystemPath(string rootDirectory)
        {
            return Path.Combine(new[] { rootDirectory }.Concat(parts).ToArray());
        }

        internal bool IsExtension(IEnumerable<string> extensions)
        {
            var last = Last;
            return last.Select(fn => extensions.Any(_ => fn.EndsWith(_, StringComparison.InvariantCultureIgnoreCase))).ValueOr(false);
        }

        internal ContentPath CatDir(string name)
        {
            return new ContentPath(parts.Concat(new[] { name }));
        }

        public Option<ContentPath> Parent
        {
            get
            {
                if (parts.Length >= 2)
                {
                    return new ContentPath(parts.Take(parts.Length - 1));
                }
                else
                {
                    return Option<ContentPath>.None;
                }
            }
        }

        public Option<string> Last => parts.Length > 0 ? parts[parts.Length - 1] : Option<string>.None;

        public Option<string> ExtensionWithoutDot => Last.Select(_ => _.Split(".", StringSplitOptions.RemoveEmptyEntries).Last());

        public Option<string> GetMimeType()
        {
            return Last.Select(MimeTypes.GetMimeType);
        }
    }
    
}