using Markdig.Helpers;
using Markdig.Parsers;

namespace SessionNotebook.Markdown;

public class NounLinkParser: InlineParser
{
    public NounLinkParser()
    {
        OpeningCharacters = ['@'];
    }

    public override bool Match(InlineProcessor processor, ref StringSlice slice)
    {
        //@[1]
        var currentChar = slice.NextChar();
        if (currentChar != '[')
        {
            return false;
        }

        currentChar = slice.NextChar();
        var startIndex = slice.Start;
        var endIndex = slice.Start;

        while (currentChar.IsDigit())
        {
            endIndex = slice.Start;
            currentChar = slice.NextChar();
        }

        if (currentChar != ']')
        {
            return false;
        }

        slice.NextChar();

        processor.Inline = new NounLink
        {
            Span =
            {
                Start = processor.GetSourcePosition(startIndex - 2, out var line, out var column),
                End = slice.Start
            },
            Line = line,
            Column = column,
            NounId = int.Parse(new StringSlice(slice.Text, startIndex, endIndex).ToString())
        };

        return true;
    }
}
