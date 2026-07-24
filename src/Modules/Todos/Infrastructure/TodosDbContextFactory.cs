using Microsoft.EntityFrameworkCore.Design;

namespace Todos.Infrastructure;

// Design-time factory so `dotnet ef migrations` can build the context without booting the host.
// The connection string is a placeholder — migrations only need the provider, not a live database.
internal sealed class TodosDbContextFactory : IDesignTimeDbContextFactory<TodosDbContext>
{
    public TodosDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<TodosDbContext>()
            .UseNpgsql(
                "Host=localhost;Database=design_time;Username=postgres;Password=postgres",
                npgsql => npgsql.MigrationsHistoryTable("__ef_migrations_history", TodosDbContext.Schema))
            .UseSnakeCaseNamingConvention()
            .Options;

        return new TodosDbContext(options);
    }
}
