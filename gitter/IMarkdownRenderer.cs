using Functional.Option;
using System.Threading.Tasks;

namespace gitter
{
    public class RendererResult
    {
        public Option<string> Title;
        public string Body;
    }

    public interface IMarkdownRenderer
    {
        Task<RendererResult> Render(string markdown);
    }
}