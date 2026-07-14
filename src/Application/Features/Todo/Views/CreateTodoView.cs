namespace Application.Features.Todo.Views;

public class CreateTodoView
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTimeOffset? Deadline { get; set; }
}
