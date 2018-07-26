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
            return String.Join(separator, parts.Select(_ => WebUtility.UrlEncode(_)));
        }

        internal static ContentPath FromUrlPath(string path)
        {
            if (String.IsNullOrEmpty(path))
            {
                return ContentPath.RootDirectory;
            }

            var cp = new ContentPath(path.Split(separator, StringSplitOptions.RemoveEmptyEntries)
                .Select(WebUtility.UrlDecode));
            return cp;
        }

        public string Href => String.Join(separator, parts.Select(WebUtility.UrlEncode));

        public string AbsoluteHref => separator + Href;

        public string VirtualPath => "~/" + Href;

        public string GetHrefRelativeTo(ContentPath parent)
        {
            if (!parent.IsDirectory)
            {
                parent = parent.Parent.Value;
            }

            int up = 0;
            for (; ; )
            {
                if (parent.IsAncestorOrSelf(this))
                {
                    break;
                }
                parent = parent.Parent.Value;
                ++up;
            }

            if (parent.Equals(this))
            {
                return String.Join(separator, Enumerable.Range(0, up).Select(_ => "..")
                    .Concat(new[]{"."})
                    .Select(_ => WebUtility.UrlEncode(_))
                    );
            }

            return String.Join(separator, Enumerable.Range(0, up).Select(_ => "..")
                .Concat(this.parts.Skip(parent.parts.Length))
                .Select(_ => WebUtility.UrlEncode(_))
                );
        }

        public ContentPath GetRelativePath(ContentPath p)
        {
            if (!IsDirectory)
            {
                return Parent.Value.GetRelativePath(p);
            }

            if (this.IsAncestorOrSelf(p))
            {
                if (p.Equals(this))
                {
                    return new ContentPath(new[] { "." });
                }
                return new ContentPath(p.parts.Skip(AsItem.parts.Length));
            }
            else
            {
                return new ContentPath(new[] { up }.Concat(Parent.Value.GetRelativePath(p).parts)); ;
            }
        }

        const string up = "..";

        public bool IsAncestorOrSelf(ContentPath child)
        {
            var i = AsItem;
            return i.parts.SequenceEqual(child.parts.Take(i.parts.Length));
        }

        public string GetFileSystemPath(string rootDirectory)
        {
            return Path.Combine(new[] { rootDirectory }.Concat(parts).ToArray());
        }

        internal bool IsExtension(IEnumerable<string> extensions)
        {
            var last = NameWithDirSlash;
            return last.Select(fn => extensions.Any(_ => fn.EndsWith(_, StringComparison.InvariantCultureIgnoreCase))).ValueOr(false);
        }

        public Option<ContentPath> CatName(string postfix)
        {
            if (parts.Length < 1)
            {
                return Option.None;
            }
            else
            {
                return new ContentPath(parts.Take(parts.Length - 1).Concat(new[] { parts.Last() + postfix }));
            }
        }

        public ContentPath CatDir(params string[] name)
        {
            return new ContentPath(this.AsItem.parts.Concat(name));
        }

        public Option<ContentPath> Parent
        {
            get
            {
                if (parts.Length <= 1)
                {
                    return Option.None;
                }

                if (IsDirectory)
                {
                    return new ContentPath(parts.Take(parts.Length - 2).Concat(new[] { DirectoryContent }));
                }
                else
                {
                    return new ContentPath(parts.Take(parts.Length - 1).Concat(new[] { DirectoryContent }));
                }
            }
        }

        public bool IsDirectory => LastPart.Select(_ => _.Equals(DirectoryContent)).ValueOr(false);

        Option<string> LastPart => (parts.Length > 0) ? parts[parts.Length - 1].ToOption() : Option.None;

        public static readonly string DirectoryContent = String.Empty;

        public Option<string> NameWithDirSlash
        {
            get
            {
                if (IsDirectory)
                {
                    if (parts.Length >= 2)
                    {
                        return parts[parts.Length - 2] + separator;
                    }
                    else
                    {
                        return root + separator;
                    }
                }
                else
                {
                    if (parts.Length >= 1)
                    {
                        return parts[parts.Length - 1];
                    }
                    else
                    {
                        return Option.None;
                    }
                }
            }
        }

        public Option<string> Name
        {
            get
            {
                if (IsDirectory)
                {
                    if (parts.Length >= 2)
                    {
                        return parts[parts.Length - 2];
                    }
                    else
                    {
                        return root;
                    }
                }
                else
                {
                    if (parts.Length >= 1)
                    {
                        return parts[parts.Length - 1];
                    }
                    else
                    {
                        return Option.None;
                    }
                }
            }
        }

        public Option<string> ExtensionWithoutDot => NameWithDirSlash.Select(_ => _.Split(".", StringSplitOptions.RemoveEmptyEntries).Last());

        public IEnumerable<ContentPath> Lineage
        {
            get
            {
                return Parent.Select(_ => _.Lineage).ValueOr(Enumerable.Empty<ContentPath>())
                    .Concat(new[] { this });
            }
        }

        public ContentPath AsDirectory
        {
            get
            {
                if (IsDirectory)
                {
                    return this;
                }
                else
                {
                    return new ContentPath(this.parts.Concat(new[] { DirectoryContent }));
                }
            }
        }

        public ContentPath AsItem
        {
            get
            {
                if (IsDirectory)
                {
                    return new ContentPath(parts.Take(parts.Length - 1));
                }
                else
                {
                    return this;
                }
            }
        }

        public static ContentPath RootDirectory => new ContentPath(new[] { DirectoryContent });

        public Option<string> GetMimeType()
        {
            return NameWithDirSlash.Select(MimeTypes.GetMimeType);
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