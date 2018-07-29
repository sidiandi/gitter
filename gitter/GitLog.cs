using Functional.Option;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace gitter
{
    public class GitLog : IHistory
    {
        private readonly IGit git;

        public GitLog(IGit git)
        {
            this.git = git;
        }

        static string UrlEncode(string markdown)
        {
            return Regex.Replace(markdown, @"urlencode\(([^)]*)\)", new MatchEvaluator(m => WebUtility.UrlEncode(m.Groups[1].Value)));
        }

        public async Task<Option<string>> GetRecentChanges(ContentPath path)
        {
            var r = await git.Run(new[] { "log", "-100", $"--pretty=format:* [%ar: %s]({path.AbsoluteHref}?log=--stat+-p+-1+-U+%H), by %an", git.GetPath(path) });
            return r.Success ? UrlEncode(r.Output) : Option<string>.None;
        }
    }
}
