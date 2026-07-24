namespace Todos.Infrastructure;

public sealed class TodosDbContext(DbContextOptions<TodosDbContext> options)
    : ModuleDbContext(options), ITodosDbContext
{
    /// <summary>Database schema owned by the Todos module.</summary>
    public const string Schema = "todos";

    protected override string ModuleSchema => Schema;

    public DbSet<TodoEntity> Todos => Set<TodoEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(TodosDbContext).Assembly);
    }
}
