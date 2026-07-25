# Observability

The host is wired for **OpenTelemetry** — traces, metrics, and logs — exported
over **OTLP**. All of it lives in one place: `Bootstrapper/Api/Infrastructure/ObservabilitySetup.cs`,
called from `Program.cs` via `services.AddObservability(builder)`.

## What's instrumented

| Signal | Sources |
|--------|---------|
| **Traces** | ASP.NET Core, HttpClient, Npgsql (PostgreSQL) |
| **Metrics** | ASP.NET Core, HttpClient, .NET runtime |
| **Logs** | ASP.NET Core logging → OTLP (scopes + formatted messages included) |

Because tracing includes **Npgsql**, every database call each module makes shows
up as a span under the request trace — no per-module wiring required.

## Configuration

Set under the `OpenTelemetry` section (see [Configuration](configuration.md)):

| Setting | Meaning | Default |
|---------|---------|---------|
| `OpenTelemetry:Endpoint` | OTLP collector endpoint | `http://localhost:4317` (dev) |
| `OpenTelemetry:ServiceName` | `service.name` resource attribute | app name |
| `OpenTelemetry:Protocol` | `grpc` or `http` (OTLP HTTP) | `grpc` |

The exporter also stamps a `deployment.environment` resource attribute from the
hosting environment name. If `Endpoint` is empty, the exporter default is used —
the integration tests set it empty so nothing is shipped during tests.

## Local backend

`docker compose up -d` starts a Grafana
[OTEL-LGTM](https://github.com/grafana/docker-otel-lgtm) container — a
ready-made **L**oki (logs) + **G**rafana + **T**empo (traces) + **M**imir/
Prometheus (metrics) stack.

| Port | Purpose |
|------|---------|
| `3003` | Grafana UI (pre-provisioned with Tempo, Loki, Prometheus data sources) |
| `4317` | OTLP gRPC |
| `4318` | OTLP HTTP |

Run the API, generate some traffic against `/api/todos`, then open
**http://localhost:3003** and explore traces in Tempo, logs in Loki, and metrics
in Prometheus.

## Switching to another collector

Point `OpenTelemetry:Endpoint` at any OTLP-compatible collector (Grafana Cloud,
Honeycomb, Jaeger with OTLP, an OpenTelemetry Collector, etc.) and set
`Protocol` to match. No code changes needed.
