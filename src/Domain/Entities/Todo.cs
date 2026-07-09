namespace Domain.Entities;

public class TodoEntity : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; } = string.Empty;
    public DateTime? Deadline { get; set; }
    public bool IsDone { get; set; }
}
