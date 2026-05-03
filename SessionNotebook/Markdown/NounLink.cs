using Markdig.Syntax.Inlines;

namespace SessionNotebook.Markdown;

public class NounLink: LeafInline
{
    public int NounId { get; set; }
}
