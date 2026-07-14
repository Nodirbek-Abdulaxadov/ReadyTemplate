using Application;
using Infrastructure;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Server.Endpoints;
using Server.Infrastructure;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;
var cfg = builder.Configuration;

services.AddEndpointsApiExplorer();
services.AddOpenApiDocument(settings =>
{
    settings.DocumentName = "v1";
    settings.Title = "ReadyTemplate swagger api docs";
});

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

var app = builder.Build();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.UseOpenApi();
    app.UseSwaggerUi();

    using var scope = app.Services.CreateScope();
    await scope.ServiceProvider.GetRequiredService<ApplicationDbContext>()
                 .Database.MigrateAsync();
}

app.UseHttpsRedirection();
app.UseCors();
app.MapTodoEndpoints();

app.Run();
