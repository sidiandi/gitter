using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace gitter
{
    public class GitContentGrep : IContentGrep
    {
        private readonly IProcessRunner runner;
        private readonly string gitRepository;

        public GitContentGrep(IProcessRunner runner, string gitRepository)
        {
            this.runner = runner;
            this.gitRepository = gitRepository;
        }

        public async Task<IEnumerable<GrepResult>> Grep(string q)
        {
            var result = await runner.Run("git.exe", new[] { "-C", gitRepository, "grep", "-I", "-n", "-i", q });
            return ParseGitGrepOutput(result.Output);
        }

        private static IEnumerable<GrepResult> ParseGitGrepOutput(string output)
        {
            using (var r = new StringReader(output))
            {
                for (; ; )
                {
                    var line = r.ReadLine();
                    if (line == null)
                    {
                        break;
                    }
                    yield return ParseGitGrepOutputLine(line);
                }
            }
        }

        private static  GrepResult ParseGitGrepOutputLine(string line)
        {
            var p = line.Split(":", 3);
            return new GrepResult
            {
                Path = ContentPath.FromUrlPath(p[0]),
                LineNumber = Int32.Parse(p[1]),
                Text = p[2]
            };

        }
    }
}
