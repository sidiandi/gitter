using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Functional.Option;
using gitter.Models;

namespace gitter
{
    class FileSystemContentProvider : IContentProvider
    {
        private readonly string rootDirectory;
        private readonly IRenderer renderer;

        public FileSystemContentProvider(string rootDirectory)
        {
            if (string.IsNullOrWhiteSpace(rootDirectory))
            {
                throw new ArgumentException("message", nameof(rootDirectory));
            }

            this.rootDirectory = rootDirectory;
        }

        public Option<Stream> Read(ContentPath path)
        {
            var fsPath = path.GetFileSystemPath(rootDirectory);

            if (File.Exists(fsPath))
            {
                return File.OpenRead(fsPath);
            }
            else
            {
                return Option.None;
            }
        }

        static private string GetDirectoryMarkdown(ContentPath path, string fsPath)
        {
            using (var r = new StringWriter())
            {
                r.WriteLine(String.Join("\n", new DirectoryInfo(fsPath).GetFileSystemInfos()
                    .Where(_ => !_.Name.Equals(".git"))
                    .Select(_ => $"* [{_.Name}]({path.CatDir(_.Name).Href})")));

                var readme = Path.Combine(fsPath, "Readme.md");
                if (File.Exists(readme))
                {
                    r.WriteLine("----");
                    r.WriteLine();
                    r.Write(File.ReadAllText(readme));
                }

                return r.ToString();
            }
        }

        public bool CanRender(ContentPath cp)
        {
            var fsPath = cp.GetFileSystemPath(rootDirectory);
            if (Directory.Exists(fsPath))
            {
                return true;
            }

            if (cp.IsImage())
            {
                return false;
            }

            if (cp.IsExtension(new[] { ".md"}))
            {
                return true;
            }

            var mimeType = cp.GetMimeType();

            return mimeType.Select(_ => !_.StartsWith("application/")).ValueOr(false);
        }

        public async Task<RawContentModel> GetRaw(ContentPath path)
        {
            var fsPath = path.GetFileSystemPath(rootDirectory);
            return new RawContentModel(path, File.OpenRead(fsPath), path.GetMimeType().ValueOr(MimeTypes.FallbackMimeType));
        }

        string GetFileSystemPath(ContentPath path)
        {
            return path.GetFileSystemPath(rootDirectory);
        }

        public IEnumerable<ContentPath> GetChildren(ContentPath path)
        {
            var fsPath = GetFileSystemPath(path);
            if (Directory.Exists(fsPath))
            {
                return new DirectoryInfo(fsPath).GetFileSystemInfos()
                    .Select(_ => path.CatDir(_.Name))
                    .ToList();
            }
            else
            {
                return Enumerable.Empty<ContentPath>();
            }
        }
    }
}
