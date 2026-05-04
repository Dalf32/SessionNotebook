namespace SessionNotebook.Data;

public static class GridFormatter
{
    public static string Format(string text)
    {
        text ??= "-";
        return text.Length > 500 ? $"{text[..500]}..." : text;
    }

    public static string Format(DateOnly? date)
    {
        return date.HasValue ? date.Value.ToString() : "-";
    }
}
