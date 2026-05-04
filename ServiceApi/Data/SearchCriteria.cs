namespace ServiceApi.Data;

public class SearchCriteria
{
    public string SortBy { get; set; }

    public string SortDir { get; set; }

    private string _query;
    public string Query
    {
        get => _query;
        set => _query = value.ToLower();
    }
    
    public int PageSize { get; set; } = 10;
    
    public int PageNumber { get; set; } = 1;

    public bool HasQuery => !string.IsNullOrEmpty(Query);

    public bool IsSortAscending => SortDir?.ToLower() == "asc";
}
