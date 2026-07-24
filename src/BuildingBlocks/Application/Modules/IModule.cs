namespace BuildingBlocks.Application.Modules;

/// <summary>
/// Contract every feature module implements so the bootstrapper can discover and wire it
/// without depending on the module's internals. A module owns its own services, endpoints,
/// and (optionally) database schema/migrations.
/// </summary>
public interface IModule
{
    /// <summary>Assembly that holds the module's handlers and validators (for mediator/FluentValidation scanning).</summary>
    Assembly Assembly { get; }

    /// <summary>Register the module's services (DbContext, features, health checks, ...).</summary>
    IServiceCollection RegisterModule(IServiceCollection services, IConfiguration configuration);

    /// <summary>Map the module's HTTP endpoints.</summary>
    IEndpointRouteBuilder MapEndpoints(IEndpointRouteBuilder endpoints);

    /// <summary>Optional startup work (e.g. applying migrations). No-op by default.</summary>
    Task InitializeAsync(IServiceProvider services) => Task.CompletedTask;
}
