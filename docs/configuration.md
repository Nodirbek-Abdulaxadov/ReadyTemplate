# Configuration

Standard ASP.NET Core configuration: `appsettings.json` +
`appsettings.{Environment}.json` + environment variables + user secrets, layered
in that order. Host config lives in `src/Bootstrapper/Api/`.

## Settings reference

| Setting | Where | Default | Notes |
|---------|-------|---------|-------|
| `ConnectionStrings:Default` | `appsettings.Development.json` | local Docker Postgres | Shared by every module — one database, one schema per module |
| `Database:AutoMigrate` | `appsettings*.json` | `true` (dev), `false` (base) | When `true`, the host applies every module's migrations on startup |
| `Cors:Origins` | `appsettings*.json` | empty | Allowed CORS origins (array); add your frontend URLs |
| `OpenTelemetry:Endpoint` | `appsettings.Development.json` | `http://localhost:4317` | OTLP collector endpoint |
| `OpenTelemetry:ServiceName` | `appsettings.Development.json` | `ReadyTemplate` | `service.name` resource attribute |
| `OpenTelemetry:Protocol` | `appsettings*.json` | `grpc` | `grpc` or `http` (OTLP HTTP) |
| `Logging:LogLevel:*` | `appsettings*.json` | Information / Warning | Standard logging config |

## Environment variables (`.env`)

`docker-compose.yml` reads these from `.env` (copy `.env.example` first):

| Variable | Default | Purpose |
|----------|---------|---------|
| `POSTGRES_PORT` | `54123` | Host port mapped to the container's `5432` |
| `POSTGRES_DB` | `template_db` | Database name |
| `POSTGRES_USER` | `postgres` | Username |
| `POSTGRES_PASSWORD` | (see `.env.example`) | Password |

> Keep the port/credentials in `.env` in sync with
> `ConnectionStrings:Default` in `appsettings.Development.json`.

## One connection string, many schemas

All modules share the single `ConnectionStrings:Default` — they live in the
**same database** but in **separate schemas** (`todos`, and one per future
module). Each module reads this same connection string in its `RegisterModule`
and scopes itself to its schema. See [Persistence](persistence.md).

If you ever want a module in a *different* database, give its `RegisterModule` a
different connection-string key — nothing else needs to change, because each
module configures its own `DbContext`.

## The `AutoMigrate` flag

- **Development** — `true`: convenient, migrations apply on startup.
- **Production** — leave `false` in the base `appsettings.json` and apply
  migrations deliberately (a release step, `dotnet ef database update`, or a
  one-shot job), so schema changes are intentional.

## Adding module-specific configuration

A module reads configuration in `RegisterModule(services, configuration)` — bind
an options class from its own section there. Keep module config under a section
named after the module (e.g. `Todos:...`) to avoid collisions, and treat that
section as part of the module's contract.

## Current request context (auth)

`ICurrentRequestService` is implemented by the host's `CurrentRequestService`.
Out of the box `UserId` is `null` (IP and user agent come from the request).
When you add authentication, populate `UserId` from the authenticated claims
there — the audit trail will pick it up automatically for every module. See
[Persistence → audit trail](persistence.md#auditinterceptor--the-audit-trail).
