namespace Todos.Application.Commands;

public record UpdateTodoCommand(UpdateTodoView View) : ICommand<TodoView>
{ }
