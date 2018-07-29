using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace gitter
{
    public class GitContentGrep : IContentGrep
    {
        private readonly IGit git;

        public GitContentGrep(IGit git)
        {
            this.git = git;
        }

        public async Task<IEnumerable<GrepResult>> Grep(string q)
        {
            var result = await git.Run(new[] { "grep", "-I", "-n", "-i", q });
            return ParseGitGrepOutput(result.Output);
        }

        internal static IEnumerable<GrepResult> ParseGitGrepOutput(string output)
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

        internal static  GrepResult ParseGitGrepOutputLine(string line)
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
