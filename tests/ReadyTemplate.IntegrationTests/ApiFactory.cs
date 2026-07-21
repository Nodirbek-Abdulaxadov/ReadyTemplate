namespace ReadyTemplate.IntegrationTests;

// App'ni test uchun ko'taradi. Real connection string'ni Testcontainer'niki bilan almashtiradi.
public sealed class ApiFactory(string connectionString) : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // InMemory source oxirida qo'shiladi -> appsettings.*.json'ni override qiladi.
        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:Default"] = connectionString,
                ["OpenTelemetry:Endpoint"] = string.Empty,   // testda tashqi OTLP yubormaymiz
            });
        });
    }
}

// Testcontainer'ni ko'taradi, app'ni bir marta warm qiladi (startup migration shunda ishlaydi).
// Collection orqali hamma integration test klassiga ulashiladi (bitta container).
public sealed class ApiFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _db = new PostgreSqlBuilder()
        .WithImage("postgres:17-alpine")
        .Build();

    public ApiFactory Factory { get; private set; } = default!;

    public async Task InitializeAsync()
    {
        await _db.StartAsync();
        Factory = new ApiFactory(_db.GetConnectionString());

        // Host'ni qur -> Program.cs'dagi DefaultDataInitializerService migration'ni ishga tushuradi.
        using var _ = Factory.CreateClient();
    }

    public async Task DisposeAsync()
    {
        await Factory.DisposeAsync();
        await _db.DisposeAsync();
    }
}

[CollectionDefinition(nameof(ApiCollection))]
#pragma warning disable CA1711 // Identifiers should not have incorrect suffix
public sealed class ApiCollection : ICollectionFixture<ApiFixture>;
#pragma warning restore CA1711 // Identifiers should not have incorrect suffix
