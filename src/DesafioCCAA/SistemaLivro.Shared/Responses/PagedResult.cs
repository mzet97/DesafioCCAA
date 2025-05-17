namespace SistemaLivro.Shared.Responses;

public class PagedResult
{
    public int CurrentPage { get; set; }
    public int PageCount { get; set; }
    public int PageSize { get; set; }
    public int RowCount { get; set; }

    public int FirstRowOnPage
    {
        get => (CurrentPage - 1) * PageSize + 1;
    }

    public int LastRowOnPage
    {
        get => Math.Min(CurrentPage * PageSize, RowCount);
    }

    public PagedResult()
    {

    }

    public PagedResult(int currentPage, int pageCount, int pageSize, int rowCount)
    {
        CurrentPage = currentPage;
        PageCount = pageCount;
        PageSize = pageSize;
        RowCount = rowCount;
    }

    public int Skip()
    {
        return (CurrentPage - 1) * PageSize;
    }

    public static PagedResult Create(int page, int pageSize, int count)
    {
        var pages = (double)count / pageSize;
        var pageCount = (int)Math.Ceiling(pages);
        return new PagedResult
        {
            CurrentPage = page,
            PageSize = pageSize,
            RowCount = count,
            PageCount = pageCount
        };
    }
}
