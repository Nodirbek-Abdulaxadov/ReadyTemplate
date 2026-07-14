namespace Infrastructure.Data;

public class ApplicationDbContext(DbContextOptions options) 
    : DbContext(options), IApplicationDbContext
{
    public DbSet<TodoEntity> Todos => Set<TodoEntity>();
    public DbSet<AuditEntity> Audits => Set<AuditEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}
