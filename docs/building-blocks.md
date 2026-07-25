# Building Blocks (the shared kernel)

`src/BuildingBlocks` is the one project every module references. It holds
cross-cutting, feature-agnostic code and knows nothing about any specific
module. It is organized to mirror a module's own inner shape — `Domain`,
`Application`, `Infrastructure` — so the concepts line up.

```
src/BuildingBlocks/
├── Domain/
│   ├── BaseEntity.cs            # Id (GUID v7), CreatedAt, UpdatedAt, Status
│   ├── AuditEntity.cs           # audit-trail row
│   ├── Status.cs                # Active / Disabled / Deleted
│   └── ActionType.cs            # Create / Update / Delete / Disable / Restore
├── Application/
│   ├── Behaviours/
│   │   └── ValidationBehaviour.cs
│   ├── Exceptions/
│   │   ├── NotFoundException.cs
│   │   └── BadRequestException.cs
│   ├── Extensions/
│   │   ├── TableOptions.cs      # paging/sorting/filter request
│   │   ├── TableResponse.cs     # paged response envelope
│   │   └── QueryExtensions.cs   # Paging / Ordering / TotalPages helpers
│   ├── Interfaces/
│   │   └── ICurrentRequestService.cs
│   └── Modules/
│       └── IModule.cs           # the module contract
├── Infrastructure/
│   ├── Persistence/
│   │   ├── ModuleDbContext.cs           # base DbContext for every module
│   │   ├── ModuleDbContextExtensions.cs # AddModuleInterceptors(...)
│   │   ├── AuditConfiguration.cs
│   │   └── Interceptors/
│   │       ├── DefaultInterceptor.cs    # timestamps + soft delete
│   │       └── AuditInterceptor.cs      # audit trail
│   └── ModuleRegistration.cs    # AddModules / MapModuleEndpoints / InitializeModulesAsync
└── Usings.cs                    # global usings for the project
```

## Domain primitives

### `BaseEntity`

Every persisted entity inherits `BaseEntity`:

```csharp
public abstract class BaseEntity
{
    public Guid Id { get; set; } = Guid.CreateVersion7();  // time-ordered, index-friendly
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public Status Status { get; set; } = Status.Active;
}
```

Inheriting it opts an entity into **automatic timestamps**, **soft delete**, and
**audit logging** — no extra code (see [Persistence](persistence.md)).

### `AuditEntity`, `Status`, `ActionType`

- `AuditEntity` — one row per tracked change (entity id, action, author, table,
  old/new JSON values, IP, user agent).
- `Status` — `Active`, `Disabled`, `Deleted` (soft-delete uses `Deleted`).
- `ActionType` — the kind of change an audit row records.

## Application abstractions

### `ICurrentRequestService`

Ambient request context the audit trail needs, implemented by the host:

```csharp
public interface ICurrentRequestService
{
    Guid? UserId { get; }
    string? IpAddress { get; }
    string? UserAgent { get; }
}
```

### Table helpers

`TableOptions` (request) and `TableResponse<T>` (response) standardize paging,
sorting, search, and date-range filtering. `QueryExtensions` provides the
`Paging`, `Ordering`, and `TotalPages` helpers used by feature classes. They are
**public** so module assemblies can consume them. See
[API & Endpoints](api-and-endpoints.md#table-queries).

### `ValidationBehaviour`

A PediatR pipeline behaviour that runs all FluentValidation validators for a
request before its handler, throwing `ValidationException` on failure. Wired
once, centrally (see [`ModuleRegistration`](#moduleregistration)). Details in
[CQRS & Validation](cqrs-validation.md).

### Exceptions

`NotFoundException` and `BadRequestException` are translated to `ProblemDetails`
by the host's global exception handler (404 / 400).

### `IModule`

The contract that lets the host discover and wire a module without touching its
internals. Full description in [Modules](modules.md).

```csharp
public interface IModule
{
    Assembly Assembly { get; }
    IServiceCollection RegisterModule(IServiceCollection services, IConfiguration configuration);
    IEndpointRouteBuilder MapEndpoints(IEndpointRouteBuilder endpoints);
    Task InitializeAsync(IServiceProvider services) => Task.CompletedTask;   // no-op by default
}
```

## Infrastructure

### `ModuleDbContext`

The base class every module's `DbContext` derives from. It pins the context to
the module's schema and gives it a private audit table:

```csharp
public abstract class ModuleDbContext(DbContextOptions options) : DbContext(options)
{
    protected abstract string ModuleSchema { get; }
    public DbSet<AuditEntity> Audits => Set<AuditEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasDefaultSchema(ModuleSchema);
        modelBuilder.ApplyConfiguration(new AuditConfiguration());
    }
}
```

### Interceptors

- `DefaultInterceptor` — sets `CreatedAt`/`UpdatedAt` and converts hard deletes
  of `BaseEntity` into soft deletes (`Status = Deleted`).
- `AuditInterceptor` — writes an `AuditEntity` row for every add/update/delete of
  a `BaseEntity`, reading the actor from `ICurrentRequestService`.

Both are `internal`. Modules attach them without seeing the types via:

```csharp
public static DbContextOptionsBuilder AddModuleInterceptors(
    this DbContextOptionsBuilder options, IServiceProvider serviceProvider);
```

### `ModuleRegistration`

The host-facing glue. Three extension methods:

```csharp
services.AddModules(configuration, modules);   // shared infra + validators + mediator + each module
endpoints.MapModuleEndpoints(modules);         // each module maps its endpoints
await serviceProvider.InitializeModulesAsync(modules);  // each module's InitializeAsync (migrations)
```

`AddModules` registers the shared `AuditInterceptor` once, then scans **all
module assemblies** for FluentValidation validators and PediatR handlers, adds
the open `ValidationBehaviour`, and finally calls each module's
`RegisterModule`. This is why adding a module needs no extra scanning wiring in
the host — see [Modules](modules.md#lifecycle).
