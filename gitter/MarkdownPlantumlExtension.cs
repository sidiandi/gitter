using Markdig;
using Markdig.Renderers;
using Markdig.Renderers.Html;
using Markdig.Syntax;
using System;
using System.IO;
using System.Linq;

namespace gitter
{
    internal class MarkdownPlantumlExtension : IMarkdownExtension
    {
        private readonly IPlantumlRenderer plantumlRenderer;

        public MarkdownPlantumlExtension(IPlantumlRenderer plantumlRenderer)
        {
            this.plantumlRenderer = plantumlRenderer;
        }

        public void Setup(MarkdownPipelineBuilder pipeline)
        {
        }

        class PlantumlRenderer : HtmlObjectRenderer<CodeBlock>
        {
            public PlantumlRenderer(IPlantumlRenderer renderer)
            {
                this.plantumlRenderer = renderer;
            }

            static bool IsPlantUml(MarkdownObject block)
            {
                var codeBlock = block as FencedCodeBlock;
                if (codeBlock == null) return false;

                var infoCorrect = codeBlock.Info.Equals("plantuml", StringComparison.InvariantCultureIgnoreCase);
                if (!infoCorrect) return false;

                return infoCorrect;
            }

            CodeBlockRenderer normalCodeBlock = new CodeBlockRenderer();
            private readonly IPlantumlRenderer plantumlRenderer;

            protected override void Write(HtmlRenderer renderer, CodeBlock obj)
            {
                if (IsPlantUml(obj))
                {
                    var fencedCodeBlock = obj as FencedCodeBlock;
                    // code block hash
                    var puml = fencedCodeBlock.Lines.ToSlice().ToString();
                    var hash = this.plantumlRenderer.GetId(puml).Result;
                    // write image link
                    var path = $"/plantuml/{hash}.png";
                    var pumlPath = $"/plantuml/{hash}.puml";
                    renderer.WriteLine($@"<img src={path.Quote()} />
<ul>
<li><a target=""_blank"" href={pumlPath.Quote()}>Source</a></li>
</ul>");
                }
                else
                {
                    normalCodeBlock.Write(renderer, obj);
                }
            }
        }

        public void Setup(MarkdownPipeline pipeline, Markdig.Renderers.IMarkdownRenderer renderer)
        {
            var htmlRenderer = renderer as HtmlRenderer;
            if (htmlRenderer != null)
            {
                htmlRenderer.ObjectRenderers.Insert(0, new PlantumlRenderer(this.plantumlRenderer));
            }
        }
    }
}