namespace Common;

public class SearchResults<T>
{
    public int PageSize { get; set; }
    
    public int TotalCount { get; set; }
    
    public required List<T> Results { get; set; }

    public int PageCount
    {
        get
        {
            if (TotalCount == 0 || PageSize == 0)
            {
                return 1;
            }
            
            return TotalCount / PageSize + (TotalCount % PageSize > 0 ? 1 : 0);
        }
    }
}
