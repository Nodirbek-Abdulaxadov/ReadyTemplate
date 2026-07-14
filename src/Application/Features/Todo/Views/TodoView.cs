namespace Application.Features.Todo.Views;

public class TodoView : CreateTodoView
{
    public Guid Id { get; set; }
    public bool IsDone { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
