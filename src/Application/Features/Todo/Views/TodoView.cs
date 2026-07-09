namespace Application.Features.Todo.Views;

public class TodoView : BaseView
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; } = string.Empty;
    public DateTime? Deadline { get; set; }
    public bool IsDone { get; set; }
}
