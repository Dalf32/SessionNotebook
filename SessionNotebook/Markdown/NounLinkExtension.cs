using DataLayer.Entities;
using Markdig;
using Markdig.Renderers;

namespace SessionNotebook.Markdown;

public class NounLinkExtension(Dictionary<int, Noun> nouns) : IMarkdownExtension
{
    public void Setup(MarkdownPipelineBuilder pipeline)
    {
        pipeline.InlineParsers.AddIfNotAlready<NounLinkParser>();
    }

    public void Setup(MarkdownPipeline pipeline, IMarkdownRenderer renderer)
    {
        if (renderer is HtmlRenderer htmlRenderer && !htmlRenderer.ObjectRenderers.Contains<NounLinkRenderer>())
        {
            htmlRenderer.ObjectRenderers.Add(new NounLinkRenderer(nouns));
        }
    }
}
