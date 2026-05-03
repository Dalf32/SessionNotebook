namespace SessionNotebook.Data;

public static class RangeExtensions
{
    public static int Length(this Range range)
    {
        return range.End.Value - range.Start.Value;
    }
}
