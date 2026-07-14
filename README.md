# ReadyTemplate

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
- 🐳 **Docker Compose** — PostgreSQL 17 with health check, configured via `.env`
- 🔄 **Auto-migrations** — database is migrated on startup in Development

## Project Structure

```
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
    ├── Endpoints/           #   Minimal API endpoint groups
    └── Infrastructure/      #   CurrentRequestService, GlobalExceptionHandler
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

## Configuration

| Setting | Where | Default |
|---------|-------|---------|
| `ConnectionStrings:Default` | `appsettings.Development.json` | local Docker Postgres |
| `Cors:Origins` | `appsettings*.json` | empty (add your frontend origins) |
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

## License

Licensed under the [MIT License](LICENSE.txt).
