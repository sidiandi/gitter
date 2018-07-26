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

        string GetFileSystemPath(ContentPath path)
        {
            return path.GetFileSystemPath(rootDirectory);
        }

        static ContentPath GetChildPath(ContentPath p, FileSystemInfo c)
        {
            if (c is DirectoryInfo)
            {
                return p.CatDir(c.Name).AsDirectory;
            }
            else
            {
                return p.CatDir(c.Name);
            }
        }

        public IEnumerable<ContentPath> GetChildren(ContentPath path)
        {
            var fsPath = GetFileSystemPath(path);
            if (Directory.Exists(fsPath))
            {
                var c = new DirectoryInfo(fsPath).GetFileSystemInfos()
                    .Where(_ => !_.Name.Equals(".git"))
                    .Select(_ => GetChildPath(path, _))
                    .OrderByDescending(_ => _.IsDirectory).ThenBy(_ => _.NameWithDirSlash.Value)
                    .ToList();
                return c;
            }
            else
            {
                return Enumerable.Empty<ContentPath>();
            }
        }

        public bool Exists(ContentPath path)
        {
            var fsPath = GetFileSystemPath(path);
            return File.Exists(fsPath) || Directory.Exists(fsPath);
        }
    }
}
