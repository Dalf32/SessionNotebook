namespace Common;

public static class StringExtensions
{
    extension(string str)
    {
        public string Capitalize()
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

        public string SpaceWords()
        {
            if (str == string.Empty)
            {
                return str;
            }

            return str.Aggregate(string.Empty,
                (s, c) => char.IsUpper(c) && !string.IsNullOrEmpty(s) ? $"{s} {c}" : $"{s}{c}");
        }

        public bool ContainsIgnoreCase(string value)
        {
            return str.Contains(value, StringComparison.CurrentCultureIgnoreCase);
        }
        
        public bool EqualsIgnoreCase(string value)
        {
            return str.Equals(value, StringComparison.CurrentCultureIgnoreCase);
        }
        
        public bool StartsWithIgnoreCase(string value)
        {
            return str.StartsWith(value, StringComparison.CurrentCultureIgnoreCase);
        }
    }
}
