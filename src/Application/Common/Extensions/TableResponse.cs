namespace Application.Common.Extensions;

public sealed class TableResponse<T> where T : class
{
    public int Total { get; set; }
    public int TotalPages { get; set; }
    public List<T> Items { get; set; } = [];
}
