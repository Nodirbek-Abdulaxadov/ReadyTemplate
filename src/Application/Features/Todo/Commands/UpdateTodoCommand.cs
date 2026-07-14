namespace Application.Features.Todo.Commands;

public record UpdateTodoCommand(UpdateTodoView View) : ICommand<TodoView>
{ }