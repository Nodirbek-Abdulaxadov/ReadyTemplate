namespace Todos.Application.Commands;

public record CreateTodoCommand(CreateTodoView View) : ICommand<TodoView>
{ }
