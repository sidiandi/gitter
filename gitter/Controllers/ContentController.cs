using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Functional.Option;
using gitter.Models;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace gitter
{
    public class ContentController : Controller
    {
        static string GetDirectoryMarkdown(IContentProvider provider, ContentPath path, IEnumerable<ContentPath> children)
        {
            return String.Join(@"
----
"
            , GetChildList(path, children).AsEnumerable().Concat(
                GetDirectoryReadme(provider, path).AsEnumerable()));
        }

        static Option<string> GetChildList(ContentPath directory, IEnumerable<ContentPath> children)
        {
            if (children.Any())
            {
                return String.Join(String.Empty, children.Select(_ => $"* [{_.NameWithDirSlash.ValueOr(_.ToString())}]({_.AbsoluteHref})\r\n"));
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

        // GET: /<controller>/
        public async Task<IActionResult> Index([FromServices] IContentProvider contentProvider, [FromServices] IMarkdownRenderer renderer, string path)
        {
            var contentPath = ContentPath.FromUrlPath(path);

            var children = contentProvider.GetChildren(contentPath);

            if (children.Any())
            {
                contentPath = contentPath.AsDirectory;
                return await MarkdownView(renderer, contentPath, GetDirectoryMarkdown(contentProvider, contentPath, children));
            }

            if (contentPath.IsExtension(new[] { ".md"}))
            {
                return await MarkdownView(renderer, contentPath, GetText(contentProvider, contentPath));
            }

            var mdFile = contentPath.CatName(".md");
            if (mdFile.HasValue && contentProvider.Exists(mdFile.Value))
            {
                return await MarkdownView(renderer, contentPath, GetText(contentProvider, mdFile.Value));
            }

            if (IsText(contentProvider, contentPath))
            {
                return await MarkdownView(renderer, contentPath, GetSourceAsMarkdown(contentProvider, contentPath));
            }

            // raw file
            return await Raw(contentProvider, path);
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

        Option<string> GetSourceAsMarkdown(IContentProvider contentProvider, ContentPath path)
        {
            return GetText(contentProvider, path).Select(_ => $@"```{path.ExtensionWithoutDot.ValueOr(String.Empty)}
{_}
```");
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
            return View("Index", new ContentModel(path, result.Title.ValueOr(path.ToString()), result.Body));
        }

        // GET: /<controller>/
        public async Task<IActionResult> Raw([FromServices] IContentProvider contentProvider, string path)
        {
            var cp = ContentPath.FromUrlPath(path);
            var content = contentProvider.Read(cp);
            var mimeType = cp.NameWithDirSlash.Select(MimeTypes.GetMimeType).ValueOr(MimeTypes.FallbackMimeType);
            return File(content.Value, mimeType);
       }
    }
}
