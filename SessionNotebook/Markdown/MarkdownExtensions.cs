using DataLayer.Entities;
using Markdig;

namespace SessionNotebook.Markdown;

public static class MarkdownExtensions
{
    public static MarkdownPipelineBuilder UseNounLinks(this MarkdownPipelineBuilder pipeline,
        Dictionary<int, Noun> nouns)
    {
        pipeline.Extensions.AddIfNotAlready(new NounLinkExtension(nouns));
        return pipeline;
    }
}
