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
        private readonly IGit git;
        private readonly IRenderer renderer;

        public FileSystemContentProvider(string rootDirectory, IGit git)
        {
            if (string.IsNullOrWhiteSpace(rootDirectory))
            {
                throw new ArgumentException("message", nameof(rootDirectory));
            }

            this.rootDirectory = rootDirectory;
            this.git = git;
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
            return p.CatDir(c.Name);
        }

        public IEnumerable<ContentPath> GetChildren(ContentPath path)
        {
            var fsPath = GetFileSystemPath(path);
            if (Directory.Exists(fsPath))
            {
                var c = new DirectoryInfo(fsPath).GetFileSystemInfos()
                    .OrderByDescending(_ => _ is DirectoryInfo).ThenBy(_ => _.Name)
                    .Where(_ => !_.Name.Equals(".git"))
                    .Select(_ => GetChildPath(path, _))
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

        DateTime nextPull = DateTime.MinValue;
        TimeSpan pullInterval = TimeSpan.FromSeconds(10);

        public Task Pull()
        {
            // itw4yclaa3btvv6psuesw5hn6zntnbw4yysczevrnhampjdseota
            if (nextPull < DateTime.UtcNow)
            {
                nextPull = DateTime.UtcNow + pullInterval;
                return git.Run(new[] { "pull" });
            }
            else
            {
                return Task.CompletedTask;
            }
        }

        public IDisposable NotifyChange(ContentPath contentPath, Action<ContentPath> onChanged)
        {
            var fsPath = this.GetFileSystemPath(contentPath);
            var directoryToWatch = fsPath = (Directory.Exists(fsPath)) ? fsPath : Path.GetDirectoryName(fsPath);
            var w = new FileSystemWatcher(directoryToWatch);
            w.BeginInit();
            FileSystemEventHandler changed = (s, e) =>
            {
                onChanged(contentPath);
            };

            w.Changed += changed;
            w.EndInit();
            w.EnableRaisingEvents = true;
            return w;
        }

        private void W_Changed(object sender, FileSystemEventArgs e)
        {
            throw new NotImplementedException();
        }
    }
}
