public class PagedList<T>
{
    public int TotalCount { get; set; }
    public required List<T> Items { get; set; }
}