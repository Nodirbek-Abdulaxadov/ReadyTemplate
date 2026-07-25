# Getting Started

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Docker](https://www.docker.com/) — for PostgreSQL locally and for the
  integration tests (Testcontainers)

## 1. Start the infrastructure

The bundled Compose stack starts PostgreSQL 17 and a Grafana
[OTEL-LGTM](https://github.com/grafana/docker-otel-lgtm) stack (Tempo, Loki,
Prometheus + Grafana) for telemetry.

```bash
cp .env.example .env   # adjust values if needed
docker compose up -d
```

| Service | URL / Port |
|---------|-----------|
| PostgreSQL | `localhost:54123` (see `.env`) |
| Grafana | http://localhost:3003 |
| OTLP gRPC | `localhost:4317` |
| OTLP HTTP | `localhost:4318` |

## 2. Run the API

```bash
dotnet run --project src/Bootstrapper/Api
```

In **Development** the host:

- applies every module's EF migrations automatically on startup
  (`Database:AutoMigrate = true`), and
- serves Swagger UI.

| Endpoint | URL |
|----------|-----|
| Swagger | http://localhost:5246/swagger |
| API base | `http://localhost:5246/api` |
| Liveness | `http://localhost:5246/healthz` |
| Readiness | `http://localhost:5246/readyz` |

## 3. Try the Todos module

```bash
# Create
curl -s -X POST http://localhost:5246/api/todos \
  -H "Content-Type: application/json" \
  -d '{"title":"Buy milk","description":"2%"}'

# List (paged)
curl -s "http://localhost:5246/api/todos?page=1&pageSize=10"
```

See [API & Endpoints](api-and-endpoints.md) for the full surface.

## 4. (Optional) Build the Docker image

```bash
docker build -f src/Bootstrapper/Api/Dockerfile -t readytemplate .
```

> The Dockerfile context is the **repository root** (it needs
> `Directory.Build.props` / `Directory.Packages.props`), so always build from
> the repo root — the `-f` flag points at the Dockerfile inside the host project.

## Solution layout

```bash
dotnet build ReadyTemplate.slnx        # build everything
dotnet test                            # run unit + integration tests
```

The solution file (`ReadyTemplate.slnx`) groups projects into `BuildingBlocks`,
`Bootstrapper`, `Modules`, and `tests`. See [Architecture](architecture.md).
