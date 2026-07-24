namespace Todos;

public sealed class TodosModule : IModule
{
    public Assembly Assembly => typeof(TodosModule).Assembly;

    public IServiceCollection RegisterModule(IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<TodosDbContext>((sp, options) => options
            .UseNpgsql(
                configuration.GetConnectionString("Default"),
                npgsql => npgsql.MigrationsHistoryTable("__ef_migrations_history", TodosDbContext.Schema))
            .UseSnakeCaseNamingConvention()
            .AddModuleInterceptors(sp));

        services.AddScoped<ITodosDbContext>(sp => sp.GetRequiredService<TodosDbContext>());
        services.AddScoped<TodoFeatures>();

        services.AddHealthChecks()
            .AddDbContextCheck<TodosDbContext>("todos-db", tags: ["ready"]);

        return services;
    }

    public IEndpointRouteBuilder MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapTodoEndpoints();
        return endpoints;
    }

    public async Task InitializeAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TodosDbContext>();
        await dbContext.Database.MigrateAsync();
    }
}
