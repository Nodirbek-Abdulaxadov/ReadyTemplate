namespace Infrastructure.Data;

public static class DefaultDataInitializerService
{
    public async static Task InitializeDatabaseAsync(this IServiceProvider services)
    {
        var scope = services.CreateScope();
        using var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await dbContext.Database.MigrateAsync();
    }
}
