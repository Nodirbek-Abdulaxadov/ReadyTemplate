namespace BuildingBlocks.Infrastructure;

/// <summary>
/// Bootstrapper-facing helpers that wire a set of <see cref="IModule"/> instances into the
/// host: shared services, per-module services, endpoint mapping, and startup initialization.
/// </summary>
public static class ModuleRegistration
{
    public static IServiceCollection AddModules(
        this IServiceCollection services, IConfiguration configuration, params IModule[] modules)
    {
        // Cross-cutting infrastructure shared by every module.
        services.AddScoped<AuditInterceptor>();

        var assemblies = modules.Select(m => m.Assembly).Distinct().ToArray();

        // Validators and mediator handlers live inside the modules; scan them once.
        services.AddValidatorsFromAssemblies(assemblies);
        services.AddPediatR(cfg =>
        {
            foreach (var assembly in assemblies)
                cfg.RegisterServicesFromAssembly(assembly);
            cfg.AddOpenBehavior(typeof(ValidationBehaviour<,>));
        });

        foreach (var module in modules)
            module.RegisterModule(services, configuration);

        return services;
    }

    public static IEndpointRouteBuilder MapModuleEndpoints(
        this IEndpointRouteBuilder endpoints, params IModule[] modules)
    {
        foreach (var module in modules)
            module.MapEndpoints(endpoints);

        return endpoints;
    }

    public static async Task InitializeModulesAsync(
        this IServiceProvider services, params IModule[] modules)
    {
        foreach (var module in modules)
            await module.InitializeAsync(services);
    }
}
