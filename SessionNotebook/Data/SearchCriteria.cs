namespace SessionNotebook.Data;

public class SearchCriteria
{
    public int? TagId { get; set; }

    public string SortBy { get; set; }

    public bool IsSortAscending { get; set; }

    public string Query { get; set; }

    public int PageSize { get; set; } = 10;
    
    public int PageNumber { get; set; } = 1;
    
    public static SearchCriteria NoPaging => new() { PageSize = 0, PageNumber = 0 };
    
    public (string paramName, string paramValue)[] GetParamPairs()
    {
        return [
            ("sortBy", SortBy),
            SortDirParam(IsSortAscending),
            ("pageSize", PageSize.ToString()),
            ("pageNumber", PageNumber.ToString()),
            ("query", Query)
        ];
    }

    public static (string paramName, string paramValue) SortDirParam(bool isAscending)
    {
        return ("sortDir", isAscending ? "asc" : "desc");
    }
}
