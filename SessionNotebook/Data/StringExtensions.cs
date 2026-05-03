namespace SessionNotebook;

public static class StringExtensions
{
    public static string Capitalize(this string str)
    {
        if (str == string.Empty)
        {
            return str;
        }

        if (str.Length == 1)
        {
            return str.ToUpper();
        }

        return char.ToUpper(str[0]) + str[1..].ToLower();
    }
}
