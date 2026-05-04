using System.Text.RegularExpressions;
using Common;
using DataLayer.Entities;

namespace SessionNotebook.Data;

public static partial class RichTextUtilities
{
    [GeneratedRegex(@"@\[(\d+)\]")]
    private static partial Regex NounIdRegex();
    public static string ProcessTextForDisplay(string noteText, List<Noun> nouns)
    {
        var processedNoteText = noteText ?? string.Empty;
        var nounMatches = NounIdRegex().Matches(processedNoteText);

        foreach (Match nounMatch in nounMatches)
        {
            var wholeMatchText = nounMatch.Value;
            var nounId = int.Parse(nounMatch.Groups.Values.Last().Value);
            var foundNoun = nouns.FirstOrDefault(n => n.Id == nounId);

            processedNoteText = processedNoteText.Replace(wholeMatchText,
                foundNoun == null ? "*[Noun unavailable]*" : $"@[{foundNoun.Name}]");
        }

        return processedNoteText;
    }

    [GeneratedRegex(@"\s")]
    private static partial Regex WhitespaceRegex();
    public static string ProcessTextForSave(string noteText, List<Noun> noteNouns)
    {
        var processedNoteText = noteText ?? string.Empty;
        var whitespace = WhitespaceRegex();

        foreach (var noun in noteNouns)
        {
            if (!whitespace.IsMatch(noun.Name))
            {
                processedNoteText = processedNoteText.Replace($"@{noun.Name}", $"@[{noun.Id}]",
                    StringComparison.CurrentCultureIgnoreCase);
            }

            processedNoteText = processedNoteText.Replace($"@[{noun.Name}]", $"@[{noun.Id}]",
                StringComparison.CurrentCultureIgnoreCase);
        }

        return processedNoteText;
    }

    [GeneratedRegex(@"@([\w'""-]+)|@\[((?:[\w'""-]+\s?)+)\]")]
    private static partial Regex NounNameRegex();
    public static List<Noun> FindNounsInText(string noteText, List<Noun> nouns, int campaignId)
    {
        var foundNouns = new List<Noun>();
        var nounMatches = NounNameRegex().Matches(noteText);

        foreach (Match nounMatch in nounMatches)
        {
            var nounText = nounMatch.Groups.Values.Skip(1).Select(m => m.Value)
                .First(mt => !string.IsNullOrEmpty(mt));
            var foundNoun = nouns.FirstOrDefault(n => n.Name.EqualsIgnoreCase(nounText));

            foundNouns.Add(foundNoun ?? new Noun { CampaignId = campaignId, Name = nounText });
        }

        return foundNouns;
    }

    [GeneratedRegex("^[^@]*]")]
    private static partial Regex ClosingBracketRegex();
    public static Range GetNounLinkAtPosition(string noteText, int position)
    {
        var invalidLink = new Range(position, position);
        if (string.IsNullOrEmpty(noteText)) return invalidLink;
        
        var textUpToPosition = noteText[..position];
        var textAfterPosition = noteText[position..];

        var atBracketPosition = textUpToPosition.LastIndexOf("@[", StringComparison.Ordinal);
        var atPosition = textUpToPosition.LastIndexOf('@');

        if (atBracketPosition == -1 && atPosition == -1)
        {
            //No @ found so no valid Noun link
            return invalidLink;
        }

        var endBracketPosition = ClosingBracketRegex().Match(textAfterPosition).Length;

        if (atBracketPosition >= atPosition)
        {
            if (noteText[atBracketPosition..position].Contains(']'))
            {
                //Closest Noun link terminates before position
                return invalidLink;
            }

            if (endBracketPosition > 0)
            {
                //Noun link opens with bracket and closes with bracket
                return new Range(atBracketPosition, position + endBracketPosition + 1);
            }
        }

        if (!string.IsNullOrEmpty(textAfterPosition) &&
            textAfterPosition[0] == '[' && endBracketPosition > 0)
        {
            //Noun link opens with bracket and closes with bracket
            return new Range(atPosition, position + endBracketPosition + 1);
        }

        if (noteText[atPosition..position].Contains(' '))
        {
            //Closest Noun link terminates before position
            return invalidLink;
        }

        var spacePosition = textAfterPosition.IndexOf(' ');
        //Noun link just runs to end of word/string
        return new Range(atPosition, position + 1 + (spacePosition >= 0 ? spacePosition : 0));
    }
}
