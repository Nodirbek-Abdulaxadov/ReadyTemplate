# ReadyTemplate

[![Build](https://github.com/Nodirbek-Abdulaxadov/ReadyTemplate/actions/workflows/build.yml/badge.svg)](https://github.com/Nodirbek-Abdulaxadov/ReadyTemplate/actions/workflows/build.yml)

A ready-to-use **.NET 10** Web API template built as a **Modular Monolith** with minimal APIs and PostgreSQL. Clone it, rename it, and start adding modules — the plumbing is already done.

## Features

- ⚡ **Minimal APIs** — endpoint groups, no controllers
- 🧱 **Modular Monolith** — self-contained feature modules behind a shared `BuildingBlocks` kernel, wired into the host through a single `IModule` contract. Each module owns its domain, application logic, endpoints, and its own database schema + migrations
- 🗄️ **Schema-per-module isolation** — every module gets its own PostgreSQL schema (e.g. `todos`) and its own migrations history table, so modules never share tables
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
- 🔁 **CI with GitHub Actions** — build with `-warnaserror` + tests + Docker image check on every push/PR
- 🧪 **Tests included** — xUnit unit tests for validators, plus integration tests that spin up a real PostgreSQL via [Testcontainers](https://testcontainers.com/) and `WebApplicationFactory`, all run in CI

## Project Structure

```
.github/workflows/build.yml    # CI: build (-warnaserror) + Docker image
Directory.Build.props          # Shared MSBuild settings for all projects
.editorconfig                  # Code style & analyzer rules
docker-compose.yml             # PostgreSQL 17 + Grafana OTEL-LGTM stack for local development
src/
├── BuildingBlocks/            # Shared kernel — cross-cutting, module-agnostic
│   ├── Domain/                #   BaseEntity, AuditEntity, Status, ActionType
│   ├── Application/
│   │   ├── Behaviours/        #   ValidationBehaviour (mediator pipeline)
│   │   ├── Exceptions/        #   NotFoundException, BadRequestException
│   │   ├── Extensions/        #   TableOptions, TableResponse, query helpers
│   │   ├── Interfaces/        #   ICurrentRequestService
│   │   └── Modules/           #   IModule — the contract every module implements
│   └── Infrastructure/
│       ├── Persistence/       #   ModuleDbContext base, AuditConfiguration,
│       │   └── Interceptors/  #   DefaultInterceptor (timestamps, soft delete)
│       │                      #   AuditInterceptor (audit trail)
│       └── ModuleRegistration #   AddModules / MapModuleEndpoints / InitializeModulesAsync
├── Modules/                   # One self-contained vertical slice per module
│   └── Todos/                 #   The reference module
│       ├── TodosModule.cs     #     IModule impl: registers services + endpoints + migrations
│       ├── Domain/            #     TodoEntity
│       ├── Application/       #     ITodosDbContext, TodoFeatures, commands, views, validators, mapper
│       ├── Infrastructure/    #     TodosDbContext (schema "todos"), configs, migrations
│       └── Endpoints/         #     TodoEndpoints (minimal API group)
└── Bootstrapper/
    └── Api/                   # ASP.NET Core host — owns no business logic
        ├── Program.cs         #   Composes the modules: `IModule[] modules = [new TodosModule()]`
        ├── Dockerfile         #   Multi-stage image build (context = repo root)
        ├── Endpoints/         #   Cross-cutting endpoints (health checks)
        └── Infrastructure/    #   CurrentRequestService, GlobalExceptionHandler,
                               #   ObservabilitySetup (OpenTelemetry wiring)
tests/
├── ReadyTemplate.UnitTests/         # xUnit + FluentValidation.TestHelper (validators)
└── ReadyTemplate.IntegrationTests/  # xUnit + Testcontainers + WebApplicationFactory
    ├── ApiFactory.cs                #   Boots the API against a throwaway Postgres container
    └── Todo/                        #   End-to-end tests for the Todo endpoints
```

## Documentation

Full docs live in [`docs/`](docs/README.md):

| Doc | What it covers |
|-----|----------------|
| [Getting Started](docs/getting-started.md) | Prerequisites, running the API, database, Swagger, telemetry |
| [Architecture](docs/architecture.md) | Modular monolith principles, dependency rules, project map |
| [Building Blocks](docs/building-blocks.md) | The shared kernel: base types, exceptions, extensions, interceptors, registration |
| [Modules](docs/modules.md) | Module anatomy, the `IModule` contract, and its lifecycle |
| [Adding a Module](docs/adding-a-module.md) | End-to-end walkthrough for a new module |
| [Persistence](docs/persistence.md) | Schema-per-module, migrations, audit trail, soft delete, GUID v7 |
| [CQRS & Validation](docs/cqrs-validation.md) | Feature classes, PediatR, FluentValidation pipeline, Mapperly |
| [API & Endpoints](docs/api-and-endpoints.md) | Minimal API groups, table queries, error handling, health checks |
| [Observability](docs/observability.md) | OpenTelemetry traces, metrics, logs over OTLP |
| [Testing](docs/testing.md) | Unit tests and Testcontainers-backed integration tests |
| [Configuration](docs/configuration.md) | `appsettings`, environment variables, CORS, feature flags |

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
dotnet run --project src/Bootstrapper/Api
```

In Development the app applies EF migrations automatically and opens Swagger UI:

- Swagger: http://localhost:5246/swagger
- API base: `http://localhost:5246/api`

Telemetry (traces, metrics, logs) is exported to the OTEL-LGTM container started by `docker compose`. Explore it in Grafana:

- Grafana: http://localhost:3003 (pre-provisioned with Tempo, Loki, and Prometheus data sources)

### 3. (Optional) Build the API as a Docker image

```bash
docker build -f src/Bootstrapper/Api/Dockerfile -t readytemplate .
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

## Adding a New Module

The `Todos` module is the reference implementation. To add e.g. a `Products` module:

1. **Create the project** — `src/Modules/Products/Products.csproj`, referencing `BuildingBlocks` (copy `Todos.csproj` as a starting point).
2. **Domain** — add a `ProductEntity` inheriting `BaseEntity`.
3. **Application** — add:
   - `IProductsDbContext` — exposes the module's `DbSet`s
   - `ProductFeatures.cs` — a feature class with `[Query]` / `[Command]` methods
   - `Commands/`, `Views/`, `Validators/`, `Mapping/` — as in `Todos`
4. **Infrastructure** — add `ProductsDbContext : ModuleDbContext` with its own `Schema` (e.g. `"products"`), plus EF configurations, then generate migrations into the module:
   ```bash
   dotnet ef migrations add InitialCreate \
     --project src/Modules/Products/Products.csproj \
     --startup-project src/Bootstrapper/Api/Api.csproj \
     --context ProductsDbContext \
     --output-dir Infrastructure/Migrations
   ```
5. **Endpoints** — add `ProductEndpoints.cs` with a `MapProductEndpoints` extension.
6. **Module class** — implement `ProductsModule : IModule` (register the `DbContext`, features, health check; map endpoints; migrate on `InitializeAsync`).
7. **Register** — add the module to the host in `src/Bootstrapper/Api/Program.cs`:
   ```csharp
   IModule[] modules = [new TodosModule(), new ProductsModule()];
   ```
   That single line is the only change to the host — validators and mediator handlers are discovered from the module assembly automatically.

Timestamps, soft delete, and audit logging work automatically for any entity inheriting `BaseEntity` — no extra code needed. Each module gets its own audit table inside its own schema.

## Testing

```bash
dotnet test
```

- **`tests/ReadyTemplate.UnitTests`** — fast, dependency-free unit tests for the `CreateTodoView` / `UpdateTodoView` validators using `FluentValidation.TestHelper`.
- **`tests/ReadyTemplate.IntegrationTests`** — full-stack tests that host the API with `WebApplicationFactory` and a disposable PostgreSQL container via [Testcontainers](https://testcontainers.com/), then exercise the Todo endpoints end to end. **Docker must be running.**

Both projects run automatically in CI via `dotnet test`.

## Code Quality & CI

- **`Directory.Build.props`** centralizes `TargetFramework`, `Nullable`, `ImplicitUsings`, and analyzer settings for every project — individual `.csproj` files only declare packages and project references.
- **.NET analyzers** run at `latest-recommended` level with `EnforceCodeStyleInBuild`; EF migrations are excluded as generated code.
- **`.editorconfig`** defines naming rules, formatting (Allman braces), and modern C# style preferences matching the codebase.
- **GitHub Actions** (`.github/workflows/build.yml`) runs on every push/PR to `master`:
  - `dotnet build -c Release -warnaserror` — any analyzer warning fails the build
  - `dotnet test` — runs the unit and integration test suites
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
| xUnit | Unit & integration test framework |
| Testcontainers.PostgreSql | Disposable PostgreSQL for integration tests |
| Microsoft.AspNetCore.Mvc.Testing | `WebApplicationFactory` in-memory host |
| AwesomeAssertions | Fluent test assertions |

## License

Licensed under the [MIT License](LICENSE.txt).
