# API & Endpoints

The API uses **minimal APIs** — endpoint groups, no controllers. Each module
maps its own endpoints; the host maps only cross-cutting ones (health checks).

## Where endpoints live

- **Module endpoints** — in the module, e.g. `Modules/Todos/Endpoints/TodoEndpoints.cs`,
  exposed via a `MapTodoEndpoints` extension and called from `TodosModule.MapEndpoints`.
- **Host endpoints** — in `Bootstrapper/Api/Endpoints/`, currently just
  `HealthEndpoints`.

The host wires them together in `Program.cs`:

```csharp
app.MapHealthEndpoints();          // host
app.MapModuleEndpoints(modules);   // each module's MapEndpoints
```

## Todos endpoints

| Method | Route | Description |
|--------|-------|-------------|
| `GET` | `/api/todos` | List with paging, sorting, search, date filter |
| `GET` | `/api/todos/{id}` | Get by id |
| `POST` | `/api/todos` | Create |
| `PUT` | `/api/todos` | Update |
| `DELETE` | `/api/todos/{id}` | Delete (soft) → `204 No Content` |

```csharp
internal static class TodoEndpoints
{
    public static IEndpointRouteBuilder MapTodoEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/todos").WithTags("Todos");

        group.MapGet("/", ([AsParameters] TableOptions options, ISender sender, CancellationToken ct)
            => sender.TodoFeatures().GetAllAsync(options, ct));
        group.MapGet("/{id:guid}", (Guid id, ISender sender, CancellationToken ct)
            => sender.TodoFeatures().GetAsync(id, ct));
        group.MapPost("/", (CreateTodoView view, ISender sender, CancellationToken ct)
            => sender.Send(new CreateTodoCommand(view), ct));
        group.MapPut("/", (UpdateTodoView view, ISender sender, CancellationToken ct)
            => sender.Send(new UpdateTodoCommand(view), ct));
        group.MapDelete("/{id:guid}", async (Guid id, ISender sender, CancellationToken ct) =>
        {
            await sender.Send(new DeleteTodoCommand(id), ct);
            return Results.NoContent();
        });

        return group;
    }
}
```

## Table queries

List endpoints accept `TableOptions` via `[AsParameters]` (bound from the query
string) and return `TableResponse<T>`.

**Request** (`TableOptions`):

```
GET /api/todos?page=1&pageSize=20&sortLabel=Title&descending=false&search=milk&from=2026-01-01&to=2026-12-31
```

| Param | Meaning | Default |
|-------|---------|---------|
| `page` | 1-based page number | `1` |
| `pageSize` | items per page (clamped to 1–100) | `10` |
| `sortLabel` | property to sort by | id |
| `descending` | sort direction | `true` |
| `search` | `LIKE` match over `Title` / `Description` | — |
| `from` / `to` | `CreatedAt` date-range filter | — |

**Response** (`TableResponse<T>`):

```json
{
  "total": 42,
  "totalPages": 3,
  "items": [ /* … */ ]
}
```

The `Paging`, `Ordering`, and `TotalPages` helpers in `QueryExtensions` (in
`BuildingBlocks`) implement this; a feature's `Sorting` switch maps `sortLabel`
to a strongly-typed key selector. See
[Building Blocks](building-blocks.md#table-helpers).

## Error handling

A single `GlobalExceptionHandler` (host) translates exceptions into RFC-7807
`ProblemDetails`:

| Exception | Status | Body |
|-----------|--------|------|
| `NotFoundException` | `404` | message as title |
| `BadRequestException` | `400` | message as title |
| `ValidationException` | `400` | title `"Validation failed"` + `errors` dictionary |
| anything else | `500` | `"Internal server error"` |

Validation errors are grouped by property:

```json
{
  "title": "Validation failed",
  "status": 400,
  "errors": {
    "Title": ["The length of 'Title' must be at least 3 characters."]
  }
}
```

Wired in `Program.cs` via `AddProblemDetails()` + `AddExceptionHandler<GlobalExceptionHandler>()`
and `app.UseExceptionHandler()`.

## Health checks

Two endpoints, defined in `HealthEndpoints`:

| Endpoint | Checks | Use |
|----------|--------|-----|
| `/healthz` | none (liveness) | "is the process up?" |
| `/readyz` | checks tagged `ready` | "can it serve traffic?" (includes each module's DB check) |

Each module registers its own readiness check, e.g.:

```csharp
services.AddHealthChecks().AddDbContextCheck<TodosDbContext>("todos-db", tags: ["ready"]);
```

Both endpoints return a JSON payload with per-check status and durations.

## Swagger / OpenAPI

NSwag serves the OpenAPI document and Swagger UI in Development only
(`app.UseOpenApi()` + `app.UseSwaggerUi()`), at `/swagger`.

## CORS

A default CORS policy reads allowed origins from `Cors:Origins` (empty by
default). Add your frontend origins in `appsettings*.json`. See
[Configuration](configuration.md).
