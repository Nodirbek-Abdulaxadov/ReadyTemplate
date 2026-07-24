namespace Todos.Application.Views;

public class UpdateTodoView : CreateTodoView
{
    public Guid Id { get; set; }
    public bool IsDone { get; set; }
}
