using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Functional.Option;
using gitter.Models;
using LibGit2Sharp;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace gitter
{
    public class ContentController : Controller
    {
        static string GetDirectoryMarkdown(IContentProvider provider, IHistory history, ContentPath path, IEnumerable<ContentPath> children)
        {
            return String.Join(hr, new[]
            {
                GetChildList(path, children),
                GetDirectoryReadme(provider, path),
                history.GetRecentChanges(path).Result
            }.WhereValue());
        }

        static Option<string> GetChildList(ContentPath directory, IEnumerable<ContentPath> children)
        {
            if (children.Any())
            {
                return String.Join(String.Empty, children.Select(_ => $"* [{_.Name.ValueOr(_.ToString())}]({_.AbsoluteHref})\r\n"));
            }
            else
            {
                return Option.None;
            }
        }

        static Option<string> GetDirectoryReadme(IContentProvider provider, ContentPath path)
        {
            var candidates = new[] { "Readme.md", "Index.md" };
            var markdown = candidates
                .Select(_ => GetText(provider, path.CatDir(_)))
                .FirstOrNone();

            return markdown;
        }

        static Option<string> GetText(IContentProvider provider, ContentPath path)
        {
            return provider.Read(path).Select(_ =>
            {
                using (var r = new StreamReader(_))
                {
                    return r.ReadToEnd();
                }
            });
        }

        const string hr = @"
----
";

        static string GetMarkdown(IContentProvider content, IHistory history, ContentPath path)
        {
            return String.Join(hr, new[] { GetText(content, path), history.GetRecentChanges(path).Result }.WhereValue());
        }

        // GET: /<controller>/
        public async Task<IActionResult> Index(
            [FromServices] IContentProvider contentProvider, 
            [FromServices] IMarkdownRenderer renderer, 
            [FromServices] IHistory history,
            [FromServices] IGit git,
            [FromQuery] string log,
            [FromQuery] string q,
            string path)
        {
            contentProvider.Pull();

            var contentPath = ContentPath.FromUrlPath(path);

            if (this.HttpContext.WebSockets.IsWebSocketRequest)
            {
                await NotifyContentChanged(this.HttpContext.WebSockets, contentProvider, contentPath);
                return Ok();
            }

            if (log != null)
            {
                var r = await git.Run(new[] { "log" }.Concat(Utils.SplitArguments(log)).Concat(new[] { "--", git.GetPath(contentPath) }));
                return await MarkdownView(renderer, contentPath, AsCode(r.Output));
            }

            if (q != null)
            {
                var pretty = $"--pretty=format:* [%ar: %s]({git.GetPath(contentPath)}?log=--stat+-p+-1+-U+%H), by %an";
                var grepLog = await git.Run(new[] { "log", pretty, "-100", "-S", q, git.GetPath(contentPath)});
                var grepText = git.GrepOutputToMarkdown((await git.Run(new[] { "grep", "-I", "-n", q })).Output);
                return await MarkdownView(renderer, contentPath, grepLog.Output + hr + grepText);
            }

            var children = contentProvider.GetChildren(contentPath);

            if (children.Any())
            {
                return await MarkdownView(renderer, contentPath, GetDirectoryMarkdown(contentProvider, history, contentPath, children));
            }

            if (contentPath.IsExtension(new[] { ".md"}))
            {
                return await MarkdownView(renderer, contentPath, GetMarkdown(contentProvider, history, contentPath));
            }

            var mdFile = contentPath.CatName(".md");
            if (mdFile.HasValue && contentProvider.Exists(mdFile.Value))
            {
                return await MarkdownView(renderer, contentPath, GetText(contentProvider, mdFile.Value));
            }

            if (IsText(contentProvider, contentPath))
            {
                return await MarkdownView(renderer, contentPath, GetSourceAsMarkdown(contentProvider, history, contentPath));
            }

            // raw file
            return await Raw(contentProvider, path);
        }

        static async Task WaitForClose(WebSocket webSocket)
        {
            while (!webSocket.CloseStatus.HasValue)
            {
                await Task.Delay(1000);
            }
        }

        private static async Task NotifyContentChanged(Microsoft.AspNetCore.Http.WebSocketManager webSockets, IContentProvider contentProvider, ContentPath contentPath)
        {
            var webSocket = await webSockets.AcceptWebSocketAsync();
            using (contentProvider.NotifyChange(contentPath, cp => {
                webSocket.SendAsync(UTF8Encoding.UTF8.GetBytes(cp.ToString()), WebSocketMessageType.Text, true, CancellationToken.None);
            }))
            {
                await WaitForClose(webSocket);
            }
        }

        static Option<string> Param(string name, string value)
        {
            return value == null
                ? Option<string>.None
                : $"--{name}={value}";
        }

        public async Task<IActionResult> Grep(
            [FromServices] IContentProvider contentProvider,
            [FromServices] IProcessRunner runner,
            [FromServices] IMarkdownRenderer renderer,
            [FromServices] IContentGrep grep,
            [FromQuery] string q)
        {
            var result = await grep.Grep(q);
            var markdown = result.Select(_ => $"* [{_.Path}]({_.Path.AbsoluteHref}#{_.LineNumber})({_.LineNumber}): `{_.Text.Truncate(256)}`").JoinLines();
            return await MarkdownView(renderer, ContentPath.FromUrlPath("/"), markdown);
        }

        Option<string> GetSourceAsMarkdown(IContentProvider contentProvider, IHistory history, ContentPath path)
        {
            return string.Join(hr, new[]
            {
                GetText(contentProvider, path).Select(_ => $@"```{path.ExtensionWithoutDot.ValueOr(String.Empty)}
{_}
```"
                ),
                history.GetRecentChanges(path).Result
            }.WhereValue());
        }

        static bool IsText(IContentProvider contentProvider, ContentPath contentPath)
        {
            var reader = contentProvider.Read(contentPath);
            if (!reader.HasValue)
            {
                return true;
            }
            using (var r = reader.Value)
            {
                var b = Read(r, 4096);
                return !b.Any(_ => _ == 0) && b.Any(_ => _ == '\n');
            }
        }

        static byte[] Read(Stream r, int count)
        {
            var b = new byte[count];
            var readCount = r.Read(b, 0, count);
            if (readCount == count)
            {
                return b;
            }
            else
            {
                var result = new byte[readCount];
                Array.Copy(b, result, readCount);
                return result;
            }
        }

        async Task<IActionResult> MarkdownView(IMarkdownRenderer renderer, ContentPath path, Option<string> markdown)
        {
            var result = await renderer.Render(markdown.ValueOr("empty"));
            return View("Index", new ContentModel(path, result.Title.ValueOr(path.GetDisplayName("Home")), result.Body));
        }

        // GET: /<controller>/
        public async Task<IActionResult> Raw([FromServices] IContentProvider contentProvider, string path)
        {
            var cp = ContentPath.FromUrlPath(path);
            var content = contentProvider.Read(cp);
            var mimeType = cp.Name.Select(MimeTypes.GetMimeType).ValueOr(MimeTypes.FallbackMimeType);
            return File(content.Value, mimeType);
       }

        static string AsCode(string c)
        {
            return $@"```
{c}
```";
        }

        [Route("/.history/commit/{commit}")]
        public async Task<IActionResult> Commit([FromServices] IMarkdownRenderer renderer, [FromServices] IGit git, string commit)
        {
            var r = await git.Run(new[] { "show", commit });
            return await MarkdownView(renderer, ContentPath.Root, AsCode(r.Output));
        }

        static string Attribute(string v)
        {
            return WebUtility.HtmlEncode(v).Quote();
        }

        [Route("/stats")]
        public async Task<IActionResult> Stats(
            [FromServices] IMarkdownRenderer renderer, 
            [FromServices] IGit git,
            [FromQuery] string q,
            [FromQuery] string since,
            [FromQuery] string until,
            [FromQuery] string author
            )
        {
            var logArgs = Utils.SplitArguments(q).Concat(new[]
            {
                Param(nameof(since), since),
                Param(nameof(until), until),
                Param(nameof(author), author)
            }.WhereValue()).ToArray();

            var c = git.GetChanges(logArgs);

            var byName = c.GroupBy(_ => _.Commit.author.Email)
                .Select(_ => new
                {
                    User = _.Key,
                    AddedLines = _.Sum(_c => _c.Stats.AddedLines),
                    RemovedLines = _.Sum(_c => _c.Stats.RemovedLines)
                })
                .OrderByDescending(_ => _.AddedLines + _.RemovedLines)
                .ToList();

            var byPath = c
                .SelectMany(_ => ContentPath.FromUrlPath(_.Stats.Path).Lineage
                    .Select(dir => new { Path = dir, Change = _}))
                .GroupBy(_ => _.Path)
                .Select(_ => new
                {
                    Path = _.Key,
                    AddedLines = _.Sum(_c => _c.Change.Stats.AddedLines),
                    RemovedLines = _.Sum(_c => _c.Change.Stats.RemovedLines)
                })
                .OrderByDescending(_ => _.AddedLines)
                .Take(100)
                .ToList();

            var markdown = $@"
<form action=""/stats"" method=""get"" >
<input name=""since"" placeholder=""since yyyy-mm-dd"" value={Attribute(since)} />
<input name=""until"" placeholder=""until yyyy-mm-dd"" value={Attribute(until)} />
<input name=""author"" placeholder=""author"" value={Attribute(author)} />
<input type=""submit"" />
</form>

## By User

{byName.MarkdownTable()
    .With("User", _ => $"[{_.User}](/stats?author={_.User})")
    .With("Added Lines", _ => _.AddedLines)
    .With("Removed Lines", _ => _.RemovedLines)
}

## By Path

{byPath.MarkdownTable()
    .With("Path", _ => $"[{_.Path}](/{_.Path})")
    .With("Added Lines", _ => _.AddedLines)
    .With("Removed Lines", _ => _.RemovedLines)
}

";

            return await MarkdownView(renderer, new ContentPath(), markdown);
        }
    }
}
