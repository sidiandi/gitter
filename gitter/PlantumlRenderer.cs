using Functional.Option;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace gitter
{
    public class PlantumlRenderer : IPlantumlRenderer
    {
        private readonly string cacheDir;
        private readonly string plantumlJar;
        const string pumlExtension = ".puml";
        const string pngExtension = ".png";
        readonly IProcessRunner processRunner;

        public PlantumlRenderer(IProcessRunner processRunner, string plantumlJar, string cacheDir)
        {
            this.plantumlJar = plantumlJar;
            this.processRunner = processRunner;
            Utils.EnsureDirectoryExists(cacheDir);
            this.cacheDir = cacheDir;
        }

        string GetCachePath(string id, string extension)
        {
            return Path.Combine(cacheDir, id + extension);
        }

        static string Hash(string x)
        {
            var sha1 = System.Security.Cryptography.SHA1.Create();
            var data = System.Text.UTF8Encoding.UTF8.GetBytes(x);
            var hash = sha1.ComputeHash(data);
            return String.Join(String.Empty, hash.Select(_ => _.ToString("x")));
        }

        public Task<string> GetId(string content)
        {
            return Task.Factory.StartNew(() =>
            {
                var id = Hash(content);
                var pumlPath = GetCachePath(id, pumlExtension);
                lock (cacheDir)
                {
                    if (!File.Exists(pumlPath))
                    {
                        File.WriteAllText(pumlPath, content);
                    }
                }
                return id;
            });
        }

        const string javaExe = @"java.exe";

        public async Task<Stream> GetPng(string id)
        {
            var imagePath = await Task.Factory.StartNew(() =>
            {
                // image path from hash
                var imageFsPath = GetCachePath(id, pngExtension);

                lock (cacheDir)
                {
                    // image exists ?
                    if (!File.Exists(imageFsPath))
                    {
                        var pumlFsPath = GetCachePath(id, pumlExtension);
                        var r = processRunner.Run(javaExe, new[] { "-jar", plantumlJar, "-o", cacheDir, pumlFsPath }).Result;
                        if (r.ExitCode != 0)
                        {
                            throw new Exception($@"plantuml failed:
ExitCode: {r.ExitCode}
Output:
{r.Output}

Error:
{r.Error}

"
                            );
                        }
                    }
                }

                return imageFsPath;
            });

            return File.OpenRead(imagePath);
        }

        public async Task<Stream> GetPlantuml(string id)
        {
            var pumlPath = GetCachePath(id, pumlExtension);
            return File.OpenRead(pumlPath);
        }
    }
}
