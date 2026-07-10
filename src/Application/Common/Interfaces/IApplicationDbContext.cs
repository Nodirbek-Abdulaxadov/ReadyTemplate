namespace Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<TodoEntity> Todos { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}