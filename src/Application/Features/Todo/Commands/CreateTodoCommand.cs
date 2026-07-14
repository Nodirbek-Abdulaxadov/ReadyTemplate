namespace Application.Features.Todo.Commands;

public record CreateTodoCommand(CreateTodoView View) : ICommand<TodoView>
{ }