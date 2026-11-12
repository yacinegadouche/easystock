namespace EasyStock.Web.ViewModels;

public interface IPagedList
{
    int Page { get; }
    int TotalPages { get; }
    int TotalCount { get; }
}

public record PagedList<T>(List<T> Items, int Page, int PageSize, int TotalCount) : IPagedList
{
    public int TotalPages => Math.Max(1, (int)Math.Ceiling(TotalCount / (double)PageSize));

    public static PagedList<T> Empty(int pageSize) => new([], 1, pageSize, 0);
}