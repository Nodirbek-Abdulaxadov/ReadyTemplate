namespace Application.Features.Todo;

public interface ITodoService
{
    [Command]
    Task<TodoView> CreateAsync(CreateTodoCommand command, CancellationToken cancellationToken = default);
    [Command]
    Task<TodoView> UpdateAsync(UpdateTodoCommand command, CancellationToken cancellationToken = default);
    [Command]
    Task<TodoView> DeleteAsync(DeleteTodoCommand command, CancellationToken cancellationToken = default);

    [Query]
    Task<TableResponse<TodoView>> GetAllAsync(TableOptions options, CancellationToken cancellationToken = default);

    [Query]
    Task<TodoView> GetAsync(Guid id, CancellationToken cancellationToken = default);
}
