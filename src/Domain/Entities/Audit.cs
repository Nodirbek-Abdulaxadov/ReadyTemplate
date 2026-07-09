namespace Domain.Entities;

public class AuditEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid EntityId { get; set; }
    public DateTime CreatedAt { get; set; }
    public ActionType Type { get; set; }
    public Guid? AuthorId { get; set; }
    public string TableName { get; set; } = string.Empty;
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
}
