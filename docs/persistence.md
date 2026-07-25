# Persistence

The template uses **EF Core 10 + PostgreSQL** with a strict rule: **each module
owns its own data**. Isolation is enforced physically, not just by convention.

## Schema per module

Every module has its own `DbContext` deriving from `ModuleDbContext`, and every
context is pinned to a dedicated PostgreSQL **schema**:

```csharp
public sealed class TodosDbContext(DbContextOptions<TodosDbContext> options)
    : ModuleDbContext(options), ITodosDbContext
{
    public const string Schema = "todos";
    protected override string ModuleSchema => Schema;

    public DbSet<TodoEntity> Todos => Set<TodoEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);   // sets default schema + audit table
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(TodosDbContext).Assembly);
    }
}
```

`ModuleDbContext.OnModelCreating` calls `modelBuilder.HasDefaultSchema(ModuleSchema)`,
so **all** of the module's tables — including its audit table — live in that
schema. For the Todos module you get:

| Table | Purpose |
|-------|---------|
| `todos.todos` | the entity |
| `todos.audit_entities` | the module's private audit trail |
| `todos.__ef_migrations_history` | the module's migration history |

Two modules never share a table, and their migration histories never collide.

## Migrations

Migrations belong to the **module project**, and the module applies them itself
on startup (`IModule.InitializeAsync`).

### Generating a migration

```bash
dotnet ef migrations add <Name> \
  --project src/Modules/Todos/Todos.csproj \
  --startup-project src/Bootstrapper/Api/Api.csproj \
  --context TodosDbContext \
  --output-dir Infrastructure/Migrations
```

- `--project` — where the migration files are written (the module).
- `--startup-project` — the host, which the tooling builds.
- `--context` — required because the solution has one `DbContext` per module.

### Design-time factory

`dotnet ef` needs to construct the context without running the app. Each module
ships an `IDesignTimeDbContextFactory`:

```csharp
internal sealed class TodosDbContextFactory : IDesignTimeDbContextFactory<TodosDbContext>
{
    public TodosDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<TodosDbContext>()
            .UseNpgsql(
                "Host=localhost;Database=design_time;Username=postgres;Password=postgres",
                npgsql => npgsql.MigrationsHistoryTable("__ef_migrations_history", TodosDbContext.Schema))
            .UseSnakeCaseNamingConvention()
            .Options;

        return new TodosDbContext(options);
    }
}
```

The connection string is a **placeholder** — generating migrations never touches
a live database.

### Per-module history table

The `MigrationsHistoryTable("__ef_migrations_history", Schema)` call (in both the
module registration and the design-time factory) keeps each module's applied-
migration bookkeeping inside its own schema. This is what lets modules migrate
independently.

### Applying migrations

In Development, `Database:AutoMigrate = true` makes the host call each module's
`InitializeAsync`, which runs `Database.MigrateAsync()`. In other environments,
apply them your usual way (startup flag, `dotnet ef database update`, or a
release step).

## The interceptors

Two `SaveChanges` interceptors run for every module context; they're attached
with one call in the module registration:

```csharp
options.AddModuleInterceptors(sp);   // DefaultInterceptor + AuditInterceptor
```

### `DefaultInterceptor` — timestamps + soft delete

For any `BaseEntity` being saved:

- **Added** → set `CreatedAt` and `UpdatedAt`.
- **Modified** → set `UpdatedAt`.
- **Deleted** → **not** physically deleted: the state is flipped to `Modified`,
  `Status` is set to `Deleted`, and `UpdatedAt` is stamped.

### Soft delete + query filter

Because deletes are soft, each entity configuration adds a global query filter so
"deleted" rows disappear from normal queries:

```csharp
builder.HasQueryFilter(x => x.Status != Status.Deleted);
```

To include soft-deleted rows in a specific query, use
`.IgnoreQueryFilters()`.

### `AuditInterceptor` — the audit trail

For every add/update/delete of a `BaseEntity`, it writes an `AuditEntity` row
into the same context (so it lands in the module's schema and commits in the same
transaction). Each row captures:

- `EntityId`, `Type` (`ActionType`), `TableName`, `CreatedAt`
- `OldValue` / `NewValue` as JSON (`jsonb` columns)
- `AuthorId`, `IpAddress`, `UserAgent` from `ICurrentRequestService`

`ActionType` is resolved from the change, including status transitions
(active→deleted = `Delete`, active→disabled = `Disable`, back to active =
`Restore`).

> The audit actor comes from `ICurrentRequestService`, implemented by the host
> (`CurrentRequestService`). `UserId` is `null` until you add authentication —
> wire it to your claims there. See [Configuration](configuration.md).

## GUID v7 keys

`BaseEntity.Id` defaults to `Guid.CreateVersion7()` — time-ordered UUIDs that
are index-friendly (sequential-ish), avoiding the write amplification of random
GUID primary keys while staying globally unique.

## Naming convention

`UseSnakeCaseNamingConvention()` (EFCore.NamingConventions) maps CLR
PascalCase to PostgreSQL `snake_case` for tables, columns, keys, and indexes —
so `TodoEntity.CreatedAt` becomes `todos.todos.created_at`.
