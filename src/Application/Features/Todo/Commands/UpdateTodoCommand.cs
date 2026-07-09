namespace Application.Features.Todo.Commands;

public record UpdateTodoCommand(TodoView View) : IRequest<TodoView>
{ }