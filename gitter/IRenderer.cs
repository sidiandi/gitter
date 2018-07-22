using Functional.Option;
using System.IO;
using System.Threading.Tasks;

namespace gitter
{
    public interface IRenderer
    {
        Task<RendererResult> Render(ContentPath path, Stream content);
    }

    public static class IRendererExtensions
    {
        public static Task<RendererResult> Render(this IRenderer renderer, ContentPath path, string content)
        {
            return renderer.Render(path, new MemoryStream(System.Text.Encoding.UTF8.GetBytes(content)));
        }
    }
}