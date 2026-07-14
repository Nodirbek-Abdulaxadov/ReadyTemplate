namespace Domain.Entities;

public class TodoEntity : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTimeOffset? Deadline { get; set; }
    public bool IsDone { get; set; }
}
