namespace ElegantSuits.Application.Common.Models;

public class PaginatedList<T> : List<T>
{
    public int PageIndex { get; private set; }
    public int TotalPages { get; private set; }
    public int TotalItems { get; private set; }
    public int PageSize { get; private set; }

    public PaginatedList(List<T> items, int count, int pageIndex, int pageSize)
    {
        PageIndex = pageIndex;
        PageSize = pageSize;
        TotalItems = count;
        TotalPages = pageSize > 0 ? (int)Math.Ceiling(count / (double)pageSize) : 0;

        AddRange(items);
    }

    public bool HasPreviousPage => PageIndex > 1;
    public bool HasNextPage => PageIndex < TotalPages;

    public static PaginatedList<T> Create(IEnumerable<T> source, int count, int pageIndex, int pageSize)
    {
        var items = source.ToList();
        return new PaginatedList<T>(items, count, pageIndex, pageSize);
    }
}
