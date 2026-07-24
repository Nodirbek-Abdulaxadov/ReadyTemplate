namespace BuildingBlocks.Infrastructure.Persistence;

public static class ModuleDbContextExtensions
{
    /// <summary>
    /// Attaches the shared save-changes interceptors (timestamps + soft delete, and the
    /// audit trail) to a module's <see cref="DbContext"/>. Keeps the interceptor types
    /// internal to the shared kernel while letting modules opt in with one call.
    /// </summary>
    public static DbContextOptionsBuilder AddModuleInterceptors(
        this DbContextOptionsBuilder options, IServiceProvider serviceProvider)
        => options.AddInterceptors(
            new DefaultInterceptor(),
            serviceProvider.GetRequiredService<AuditInterceptor>());
}
