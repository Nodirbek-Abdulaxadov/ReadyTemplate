using Npgsql;
using OpenTelemetry.Exporter;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace Server.Infrastructure;

public static class ObservabilitySetup
{
    public static IServiceCollection AddObservability(
        this IServiceCollection services, IHostApplicationBuilder builder)
    {
        var cfg = builder.Configuration.GetSection("OpenTelemetry");
        string serviceName = cfg["ServiceName"] ?? builder.Environment.ApplicationName;
        string? endpoint = cfg["Endpoint"];
        var protocol = (cfg["Protocol"] ?? "grpc").Equals("http", StringComparison.OrdinalIgnoreCase)
            ? OtlpExportProtocol.HttpProtobuf
            : OtlpExportProtocol.Grpc;

        void Otlp(OtlpExporterOptions o)
        {
            if (!string.IsNullOrWhiteSpace(endpoint)) o.Endpoint = new Uri(endpoint);
            o.Protocol = protocol;
        }

        var attrs = new Dictionary<string, object>
        {
            ["deployment.environment"] = builder.Environment.EnvironmentName
        };

        _ = services.AddOpenTelemetry()
            .ConfigureResource(r => r.AddService(serviceName).AddAttributes(attrs))
            .WithTracing(t => t
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddNpgsql()
                .AddOtlpExporter(Otlp))
            .WithMetrics(m => m
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddRuntimeInstrumentation()
                .AddOtlpExporter(Otlp));

        _ = builder.Logging.AddOpenTelemetry(o =>
        {
            o.IncludeScopes = true;
            o.IncludeFormattedMessage = true;
            _ = o.SetResourceBuilder(ResourceBuilder.CreateDefault()
                    .AddService(serviceName).AddAttributes(attrs))
                 .AddOtlpExporter(Otlp);
        });

        return services;
    }
}
