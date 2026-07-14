namespace Infrastructure.Data.Interceptors;

internal sealed class DefaultInterceptor : SaveChangesInterceptor
{
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData, InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context is { } context)
        {
            var now = DateTime.UtcNow;
            foreach (var entry in context.ChangeTracker.Entries<BaseEntity>())
            {
                if (entry.State == EntityState.Added)
                    entry.Entity.CreatedAt = now;
                if (entry.State is EntityState.Added or EntityState.Modified)
                    entry.Entity.UpdatedAt = now;
                if (entry.State is EntityState.Deleted)
                {
                    entry.State = EntityState.Modified;
                    entry.Entity.Status = Status.Deleted;
                    entry.Entity.UpdatedAt = now;
                }
            }
        }
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }
}
