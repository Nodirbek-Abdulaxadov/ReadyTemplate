var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;
var cfg = builder.Configuration;

services.AddEndpointsApiExplorer();
services.AddOpenApiDocument(settings =>
{
    settings.DocumentName = "v1";
    settings.Title = "ReadyTemplate swagger api docs";
});

services.AddHealthChecks();
services.AddHttpContextAccessor();
services.AddScoped<ICurrentRequestService, CurrentRequestService>();
services.AddApplication();
services.AddInfrastructure(cfg);

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

app.MapHealthEndpoints();
app.UseHttpsRedirection();
app.UseCors();
app.MapTodoEndpoints();

await DefaultDataInitializerService.InitializeDatabaseAsync(app.Services);

app.Run();
