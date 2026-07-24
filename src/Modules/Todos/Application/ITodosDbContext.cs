namespace Todos.Application;

public interface ITodosDbContext
{
    DbSet<TodoEntity> Todos { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
