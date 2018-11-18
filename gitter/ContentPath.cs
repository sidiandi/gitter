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

        public ContentPath(IEnumerable<string> parts)
        {
            this.parts = parts.ToArray();
        }

        public ContentPath(params string[] parts)
        {
            this.parts = parts;
        }

        string[] parts;

        public IEnumerable<string> Parts => parts;

        public override string ToString()
        {
            return String.Join(separator, parts.Select(_ => WebUtility.UrlEncode(_)));
        }

        internal static ContentPath FromUrlPath(string path)
        {
            if (String.IsNullOrEmpty(path))
            {
                return ContentPath.Root;
            }

            var cp = new ContentPath(path.Split(separator, StringSplitOptions.RemoveEmptyEntries)
                .Select(WebUtility.UrlDecode));
            return cp;
        }

        public string Href => String.Join(separator, parts.Select(Uri.EscapeUriString));

        public string AbsoluteHref => separator + Href;

        public string VirtualPath => "~/" + Href;

        const string up = "..";

        public bool IsAncestorOrSelf(ContentPath child)
        {
            return parts.SequenceEqual(child.parts.Take(parts.Length));
        }

        public string GetFileSystemPath(string rootDirectory)
        {
            return Path.Combine(new[] { rootDirectory }.Concat(parts).ToArray());
        }

        internal bool IsExtension(IEnumerable<string> extensions)
        {
            var last = Name;
            return last.Select(fn => extensions.Any(_ => fn.EndsWith(_, StringComparison.InvariantCultureIgnoreCase))).ValueOr(false);
        }

        public Option<ContentPath> CatName(string postfix)
        {
            return Parent.Select(_ => _.CatDir(Name + postfix ));
        }

        public ContentPath CatDir(params string[] name)
        {
            return new ContentPath(parts.Concat(name));
        }

        public Option<ContentPath> Parent
        {
            get
            {
                return IsRoot
                    ? Option<ContentPath>.None
                    : new ContentPath(parts.Take(parts.Length - 1));
            }
        }

        public Option<string> Name
        {
            get
            {
                return IsRoot
                    ? Option<string>.None
                    : parts[parts.Length - 1];
            }
        }

        public Option<string> ExtensionWithoutDot => Name.Select(_ => _.Split(".", StringSplitOptions.RemoveEmptyEntries).Last());

        public IEnumerable<ContentPath> Lineage
        {
            get
            {
                return GetLineage().ToList();
            }
        }

        IEnumerable<ContentPath> GetLineage()
        {
            return Parent.Select(_ => _.GetLineage()).ValueOr(Enumerable.Empty<ContentPath>())
                .Concat(new[] { this });
        }

        public static ContentPath Root => new ContentPath(new string[] {});

        public bool IsRoot => parts.Length == 0;

        public Option<string> GetMimeType()
        {
            return Name.Select(MimeTypes.GetMimeType);
        }

        public string GetDisplayName(string rootName)
        {
            return IsRoot
                ? rootName
                : Name.Value;
        }

        public override bool Equals(object obj)
        {
            var r = obj as ContentPath;
            if (r == null) return false;
            return parts.SequenceEqual(r.parts);
        }

        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }
    }
    
}