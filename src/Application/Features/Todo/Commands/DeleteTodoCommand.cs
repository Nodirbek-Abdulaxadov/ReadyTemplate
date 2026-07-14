namespace Application.Features.Todo.Commands;

public record DeleteTodoCommand(Guid Id) : ICommand<Guid>
{ }