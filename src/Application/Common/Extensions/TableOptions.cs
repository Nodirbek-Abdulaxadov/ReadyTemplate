namespace Application.Common.Extensions;

public class TableOptions
{
    public int PageSize
    {
        get;
        set => field = Math.Clamp(value, 1, 100);
    } = 10;
    public int Page { get; set; } = 1;
    public string? SortLabel { get; set; }
    public bool? Descending { get; set; } = true;
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }
    public string? Search { get; set; }
}
