using DataLayer.Entities;
using Markdig.Renderers;
using Markdig.Renderers.Html;

namespace SessionNotebook.Markdown;

public class NounLinkRenderer(Dictionary<int, Noun> nouns) : HtmlObjectRenderer<NounLink>
{
    protected override void Write(HtmlRenderer renderer, NounLink nounLink)
    {
        if (nouns.TryGetValue(nounLink.NounId, out var noun))
        {
            var synopsisAttribute = string.IsNullOrEmpty(noun.Synopsis)
                ? string.Empty
                : $"synopsis=\"{noun.Synopsis}\"";
            renderer.Write(
                $"<a class=\"noun-link\" {synopsisAttribute} href=\"/noun/{noun.Id}\">{noun.Name}</a>");
        }
        else
        {
            renderer.Write("<em>[Noun unavailable]</em>");
        }
    }
}
