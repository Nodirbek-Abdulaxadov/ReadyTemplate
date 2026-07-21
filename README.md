# ReadyTemplate

[![Build](https://github.com/Nodirbek-Abdulaxadov/ReadyTemplate/actions/workflows/build.yml/badge.svg)](https://github.com/Nodirbek-Abdulaxadov/ReadyTemplate/actions/workflows/build.yml)

A ready-to-use **.NET 10** Web API template built with Clean Architecture, minimal APIs, and PostgreSQL. Clone it, rename it, and start building features — the plumbing is already done.

## Features

- ⚡ **Minimal APIs** — endpoint groups, no controllers
- 🧅 **Clean Architecture** — `Domain` → `Application` → `Infrastructure` → `Server`
- 📬 **CQRS with [PediatR](https://www.nuget.org/packages/PediatR)** — lightweight MediatR-compatible mediator with `[Query]` / `[Command]` feature classes
- ✅ **FluentValidation** — wired into the pipeline via `ValidationBehaviour`
- 🗺️ **[Mapperly](https://mapperly.riok.app/)** — compile-time source-generated mapping (zero reflection)
- 🐘 **EF Core 10 + PostgreSQL** — with `snake_case` naming convention
- 🕵️ **Audit logging** — every create/update/delete is recorded automatically (old/new values, user id, IP, user agent) via a `SaveChanges` interceptor
- 🗑️ **Soft delete** — deletes are converted to `Status.Deleted`, timestamps (`CreatedAt`/`UpdatedAt`) are set automatically
- 🔑 **GUID v7 IDs** — time-ordered, index-friendly primary keys
- 📄 **Table queries out of the box** — paging, sorting, search, and date-range filtering via `TableOptions` / `TableResponse<T>`
- 🚨 **Global exception handling** — `ProblemDetails` responses with custom `NotFoundException` / `BadRequestException`
- 📖 **Swagger (NSwag)** — enabled in Development
- 🐳 **Docker** — multi-stage `Dockerfile` for the API + Docker Compose for PostgreSQL 17
- 📊 **OpenTelemetry** — tracing, metrics, and logging for ASP.NET Core, HttpClient, and Npgsql, exported over OTLP to a bundled Grafana [OTEL-LGTM](https://github.com/grafana/docker-otel-lgtm) stack
- 🔄 **Auto-migrations** — database is migrated on startup in Development
- 🧹 **Code quality built in** — `Directory.Build.props` with .NET analyzers (`latest-recommended`) + a full `.editorconfig`, enforced on every build
- 🔁 **CI with GitHub Actions** — build with `-warnaserror` + Docker image check on every push/PR

## Project Structure

```
.github/workflows/build.yml  # CI: build (-warnaserror) + Docker image
Directory.Build.props        # Shared MSBuild settings for all projects
.editorconfig                # Code style & analyzer rules
docker-compose.yml           # PostgreSQL 17 + Grafana OTEL-LGTM stack for local development
src/
├── Domain/                  # Entities, enums, base types — no dependencies
│   ├── Common/              #   BaseEntity (Id, CreatedAt, UpdatedAt, Status)
│   ├── Entities/            #   Todo, Audit
│   └── Enums/               #   Status, ActionType
├── Application/             # Business logic
│   ├── Common/
│   │   ├── Behaviours/      #   ValidationBehaviour (pipeline)
│   │   ├── Exceptions/      #   NotFoundException, BadRequestException
│   │   ├── Extensions/      #   TableOptions, TableResponse, query helpers
│   │   └── Interfaces/      #   IApplicationDbContext, ICurrentRequestService
│   └── Features/
│       └── Todo/            #   Feature slice: commands, validators, views, mapper
├── Infrastructure/          # Data access
│   ├── Data/
│   │   ├── Configurations/  #   EF entity configurations
│   │   └── Interceptors/    #   DefaultInterceptor (timestamps, soft delete)
│   │                        #   AuditInterceptor (audit trail)
│   └── Migrations/
└── Server/                  # ASP.NET Core host
    ├── Dockerfile           #   Multi-stage image build (context = repo root)
    ├── Endpoints/           #   Minimal API endpoint groups
    └── Infrastructure/      #   CurrentRequestService, GlobalExceptionHandler,
                             #   ObservabilitySetup (OpenTelemetry wiring)
```

## Getting Started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Docker](https://www.docker.com/) (for PostgreSQL)

### 1. Start the database

```bash
cp .env.example .env   # adjust values if needed
docker compose up -d
```

### 2. Run the API

```bash
dotnet run --project src/Server
```

In Development the app applies EF migrations automatically and opens Swagger UI:

- Swagger: http://localhost:5246/swagger
- API base: `http://localhost:5246/api`

Telemetry (traces, metrics, logs) is exported to the OTEL-LGTM container started by `docker compose`. Explore it in Grafana:

- Grafana: http://localhost:3003 (pre-provisioned with Tempo, Loki, and Prometheus data sources)

### 3. (Optional) Build the API as a Docker image

```bash
docker build -f src/Server/Dockerfile -t readytemplate .
```

> The Dockerfile context is the repo root (it needs `Directory.Build.props`), so always build from the repository root.

## Example Endpoints (Todo)

| Method | Route | Description |
|--------|-------|-------------|
| `GET` | `/api/todos` | List with paging, sorting, search, date filter |
| `GET` | `/api/todos/{id}` | Get by id |
| `POST` | `/api/todos` | Create |
| `PUT` | `/api/todos` | Update |
| `DELETE` | `/api/todos/{id}` | Delete (soft) |

List query parameters (`TableOptions`):

```
GET /api/todos?page=1&pageSize=20&sortLabel=Title&descending=false&search=milk&from=2026-01-01&to=2026-12-31
```

Response shape (`TableResponse<T>`):

```json
{
  "total": 42,
  "totalPages": 3,
  "items": [ ... ]
}
```

## Adding a New Feature

The `Todo` feature is the reference implementation. To add e.g. `Product`:

1. **Domain** — add `Product` entity inheriting `BaseEntity`, plus an EF configuration in `Infrastructure/Data/Configurations`.
2. **Application** — create `Features/Product/` with:
   - `ProductFeatures.cs` — a feature class with `[Query]` / `[Command]` methods
   - `Commands/` — command records wrapping the views
   - `Views/` — request/response DTOs
   - `Validators/` — FluentValidation validators (run automatically in the pipeline)
   - `Mapping/` — a Mapperly `[Mapper]` class
3. **Infrastructure** — add `DbSet<Product>` to `ApplicationDbContext` / `IApplicationDbContext`, then:
   ```bash
   dotnet ef migrations add AddProduct --project src/Infrastructure --startup-project src/Server
   ```
4. **Server** — add `ProductEndpoints.cs` and map it in `Program.cs`.
5. **Register** — add `services.AddScoped<ProductFeatures>()` in `Application/DependencyInjection.cs`.

Timestamps, soft delete, and audit logging work automatically for any entity inheriting `BaseEntity` — no extra code needed.

## Code Quality & CI

- **`Directory.Build.props`** centralizes `TargetFramework`, `Nullable`, `ImplicitUsings`, and analyzer settings for every project — individual `.csproj` files only declare packages and project references.
- **.NET analyzers** run at `latest-recommended` level with `EnforceCodeStyleInBuild`; EF migrations are excluded as generated code.
- **`.editorconfig`** defines naming rules, formatting (Allman braces), and modern C# style preferences matching the codebase.
- **GitHub Actions** (`.github/workflows/build.yml`) runs on every push/PR to `master`:
  - `dotnet build -c Release -warnaserror` — any analyzer warning fails the build
  - `docker build` — verifies the API image still builds

## Configuration

| Setting | Where | Default |
|---------|-------|---------|
| `ConnectionStrings:Default` | `appsettings.Development.json` | local Docker Postgres |
| `Cors:Origins` | `appsettings*.json` | empty (add your frontend origins) |
| `OpenTelemetry:Endpoint` | `appsettings.Development.json` | `http://localhost:4317` (OTLP gRPC) |
| `OpenTelemetry:ServiceName` | `appsettings.Development.json` | `ReadyTemplate` |
| `OpenTelemetry:Protocol` | `appsettings*.json` | `grpc` (or `http` for OTLP HTTP) |
| `POSTGRES_PORT` / `POSTGRES_DB` / `POSTGRES_USER` / `POSTGRES_PASSWORD` | `.env` | see `.env.example` |

## Tech Stack

| Package | Purpose |
|---------|---------|
| PediatR | CQRS mediator |
| FluentValidation | Request validation |
| Riok.Mapperly | Source-generated mapping |
| Npgsql.EntityFrameworkCore.PostgreSQL | EF Core provider |
| EFCore.NamingConventions | `snake_case` tables/columns |
| NSwag.AspNetCore | OpenAPI / Swagger UI |
| OpenTelemetry (+ AspNetCore / Http / Runtime / Npgsql instrumentation) | Traces, metrics & logs over OTLP |

## License

Licensed under the [MIT License](LICENSE.txt).
