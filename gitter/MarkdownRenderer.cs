using Functional.Option;
using Markdig;
using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace gitter
{
    internal class MarkdownRenderer : IMarkdownRenderer
    {
        readonly Markdig.MarkdownPipeline markdownPipeline;
        private readonly IPlantumlRenderer plantumlRenderer;

        public MarkdownRenderer(IPlantumlRenderer plantumlRenderer)
        {
            var b = new Markdig.MarkdownPipelineBuilder();
            b.UseAdvancedExtensions();
            b.Extensions.Add(new MarkdownPlantumlExtension(plantumlRenderer));
            markdownPipeline = b.Build();
            this.plantumlRenderer = plantumlRenderer;
        }

        static Option<string> GetTitle(string markdown)
        {
            return Regex.Match(markdown, @"^#\s+(.*)$", RegexOptions.Multiline)
                .AsOption().Select(_ => _.Groups[1].Value);
        }

        public async Task<RendererResult> Render(string markdown)
        {
            return new RendererResult
            {
                Body = Markdig.Markdown.ToHtml(markdown, markdownPipeline),
                Title = GetTitle(markdown)
            };
        }
    }
}