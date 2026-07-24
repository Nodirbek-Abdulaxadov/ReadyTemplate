var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;
var cfg = builder.Configuration;

// The set of feature modules that make up this monolith. Adding a new module is a
// one-line change here — everything else it needs it wires up itself via IModule.
IModule[] modules = [new TodosModule()];

services.AddEndpointsApiExplorer();
services.AddOpenApiDocument(settings =>
{
    settings.DocumentName = "v1";
    settings.Title = "ReadyTemplate swagger api docs";
});

services.AddHealthChecks();
services.AddHttpContextAccessor();
services.AddScoped<ICurrentRequestService, CurrentRequestService>();
services.AddModules(cfg, modules);

services.AddCors(o => o.AddDefaultPolicy(p => p
    .WithOrigins(cfg.GetSection("Cors:Origins").Get<string[]>() ?? [])
    .AllowAnyHeader()
    .AllowAnyMethod()));

services.AddProblemDetails();
services.AddExceptionHandler<GlobalExceptionHandler>();
services.AddObservability(builder);

var app = builder.Build();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.UseOpenApi();
    app.UseSwaggerUi();
}
else
{
    app.UseHsts();
}

app.MapHealthEndpoints();
app.UseHttpsRedirection();
app.UseCors();
app.MapModuleEndpoints(modules);

if (cfg.GetValue<bool>("Database:AutoMigrate"))
    await app.Services.InitializeModulesAsync(modules);

app.Run();

// Exposed for WebApplicationFactory<Program> in the integration tests.
public partial class Program;
