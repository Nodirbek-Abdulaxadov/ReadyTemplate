namespace BuildingBlocks.Infrastructure.Persistence;

/// <summary>
/// Base class for every module's <see cref="DbContext"/>. Pins the module to its own
/// database schema and gives it a private audit table, keeping modules isolated at the
/// data layer. Derived contexts add their own entities via
/// <c>ApplyConfigurationsFromAssembly</c>.
/// </summary>
public abstract class ModuleDbContext(DbContextOptions options) : DbContext(options)
{
    /// <summary>Database schema that owns all of this module's tables.</summary>
    protected abstract string ModuleSchema { get; }

    public DbSet<AuditEntity> Audits => Set<AuditEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasDefaultSchema(ModuleSchema);
        modelBuilder.ApplyConfiguration(new AuditConfiguration());
    }
}
