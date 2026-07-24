namespace Todos.Application.Commands;

public record DeleteTodoCommand(Guid Id) : ICommand<Guid>
{ }
