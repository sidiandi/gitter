using Functional.Option;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace gitter
{
    public interface IGit
    {
        Task<ProcessResult> Run(IEnumerable<string> arguments);
    }

    public static class IGitExtensions
    {
        public static string GetPath(this IGit git, ContentPath path)
        {
            return path.IsRoot
                ? "."
                : String.Join("/", path.Parts);
        }

        public static string GrepOutputToMarkdown(this IGit git, string grepOutput)
        {
            var result = GitContentGrep.ParseGitGrepOutput(grepOutput);
            var markdown = result.Select(_ => $"* [{_.Path}]({_.Path.AbsoluteHref}#{_.LineNumber})({_.LineNumber}): `{_.Text.Truncate(256)}`").JoinLines();
            return markdown;
        }

        public class Change
        {
            public Stats Stats;
            public Commit Commit;
        }

        public static IEnumerable<Change> GetChanges(this IGit git, params string[] logArguments)
        {
            var r = git.Run(new[] { "log", "--no-merges", "--pretty=raw", "--numstat" }.Concat(logArguments)).Result;
            using (var reader = new StringReader(r.Output))
            {
                return ReadChanges(reader).ToList();
            }
        }

        public static IEnumerable<Change> ReadChanges(TextReader reader)
        {
            for (; ; )
            {
                var c = IGitExtensions.ReadRawCommit(reader);
                if (!c.HasValue)
                {
                    break;
                }
                var s = IGitExtensions.ReadNumStat(reader);

                foreach (var i in s)
                {
                    yield return new Change
                    {
                        Stats = i,
                        Commit = c.Value
                    };
                }
            }
        }

        public class CommitName
        {
            public string Name;
            public string Email;
            public DateTime Time;
        }

        public static CommitName ParseCommitName(string text)
        {
            var mailBegin = text.IndexOf('<');
            var mailEnd = text.IndexOf('>');
            var timeParts = Regex.Split(text.Substring(mailEnd + 1), @"\s+");

            return new CommitName
            {
                Name = text.Substring(0, mailBegin - 1).Trim(),
                Email = text.Substring(mailBegin + 1, mailEnd - 1 - mailBegin),
                Time = new DateTime(1970, 1, 1) + TimeSpan.FromSeconds(Int32.Parse(timeParts[1]))
            };
        }

        public class Commit
        {
            public string commit;
            public string tree;
            public string[] parent;
            public CommitName author;
            public CommitName committer;
            public string message;
        }

        public static IEnumerable<string> GetLines(TextReader reader)
        {
            for (; ; )
            {
                var line = reader.ReadLine();
                if (line == null)
                {
                    break;
                }
                yield return line;
            }
        }

        static IEnumerable<string> ConsumeWhile(IEnumerator<string> lines, Func<string, bool> mustBeTrue)
        {
            for (; ;)
            {
                if (mustBeTrue(lines.Current))
                {
                    yield return lines.Current;
                    if (!lines.MoveNext())
                    {
                        break;
                    }
                }
                else
                {
                    break;
                }
            }
        }

        static IEnumerable<string> ReadUntilEmptyLine(TextReader r)
        {
            for (; ; )
            {
                var line = r.ReadLine();
                if (String.IsNullOrEmpty(line))
                {
                    break;
                }
                yield return line;
            }
        }

        static ILookup<string, string> ReadHeader(TextReader r)
        {
            return ReadUntilEmptyLine(r)
                .Select(_ => _.Split(" ", 2))
                .ToLookup(_ => _[0], _ => _[1]);
        }

        static string ReadCommitMessage(TextReader reader)
        {
            const string prefix = "    ";
            var m = new StringWriter();
            foreach (var i in ReadUntilEmptyLine(reader)
                .Select(_ => _.Substring(prefix.Length)))
            {
                m.WriteLine(i);
            }
            return m.ToString();
        }

        public static Option<Commit> ReadRawCommit(TextReader reader)
        {
            var header = ReadHeader(reader);
            if (header.Count == 0)
            {
                return Option.None;
            }

            var c = new Commit
            {
                author = ParseCommitName(header["author"].Single()),
                tree = header["tree"].Single(),
                committer = ParseCommitName(header["committer"].Single()),
                parent = header["parent"].ToArray(),
                message = ReadCommitMessage(reader)
            };
            
            return c;
        }

        public class Stats
        {
            public string Path;
            public int RemovedLines;
            public int AddedLines;
        }

        static Stats ParseNumStat(string line)
        {
            var p = line.Split("\t", 3);
            return new Stats
            {
                Path = p[2],
                RemovedLines = ParseNumStatInt(p[1]),
                AddedLines = ParseNumStatInt(p[0])
            };
        }

        static int ParseNumStatInt(string x)
        {
            if (Int32.TryParse(x, out var i))
            {
                return i;
            }
            else
            {
                return 0;
            }
        }

        public static IEnumerable<Stats> ReadNumStat(TextReader reader)
        {
            var nextChar = (char) reader.Peek();

            if (Char.IsDigit(nextChar) || nextChar == '-')
            {
                return ReadUntilEmptyLine(reader)
                    .Select(ParseNumStat)
                    .ToList();
            }
            else
            {
                return System.Linq.Enumerable.Empty<Stats>();
            }

        }
    }
}
