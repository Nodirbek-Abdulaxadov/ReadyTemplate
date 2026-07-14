using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Text.Json;

namespace Infrastructure.Data.Interceptors;

internal sealed class AuditInterceptor(ICurrentRequestService currentRequest) : SaveChangesInterceptor
{
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData, InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context is { } context)
            WriteAudits(context);

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void WriteAudits(DbContext context)
    {
        var now = DateTime.UtcNow;
        var audits = new List<AuditEntity>();

        foreach (var entry in context.ChangeTracker.Entries<BaseEntity>())
        {
            if (entry.State is not (EntityState.Added or EntityState.Modified or EntityState.Deleted))
                continue;

            audits.Add(new AuditEntity
            {
                EntityId = entry.Entity.Id,
                CreatedAt = now,
                Type = ResolveAction(entry),
                AuthorId = currentRequest.UserId,
                TableName = entry.Metadata.GetTableName() ?? entry.Metadata.ClrType.Name,
                OldValue = entry.State == EntityState.Added ? null : Serialize(entry.OriginalValues),
                NewValue = entry.State == EntityState.Deleted ? null : Serialize(entry.CurrentValues),
                IpAddress = currentRequest.IpAddress,
                UserAgent = currentRequest.UserAgent
            });
        }

        if (audits.Count > 0)
            context.Set<AuditEntity>().AddRange(audits);
    }

    private static ActionType ResolveAction(EntityEntry<BaseEntity> entry)
    {
        if (entry.State == EntityState.Added) return ActionType.Create;
        if (entry.State == EntityState.Deleted) return ActionType.Delete;

        var oldStatus = (Status)entry.OriginalValues[nameof(BaseEntity.Status)]!;
        return (oldStatus, entry.Entity.Status) switch
        {
            (not Status.Deleted, Status.Deleted) => ActionType.Delete,
            (not Status.Disabled, Status.Disabled) => ActionType.Disable,
            (Status.Deleted or Status.Disabled, Status.Active) => ActionType.Restore,
            _ => ActionType.Update
        };
    }

    private static string Serialize(PropertyValues values)
        => JsonSerializer.Serialize(
            values.Properties.ToDictionary(p => p.Name, p => values[p]));
}