namespace Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<TodoEntity> Todos { get; }
    DbSet<AuditEntity> Audits { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}