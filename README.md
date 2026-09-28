# BookIt backend

BookIt is a .NET 10 API for a small book-lending library. It currently includes the initial PostgreSQL schema, member registration and login, stateless JWT access/refresh tokens, Swagger, health checks, and a local Docker Compose environment with .NET watch mode.

## Architecture

```text
BookIt.Api -> BookIt.Infrastructure -> BookIt.Application -> BookIt.Domain
           \-> BookIt.Application
```

- `BookIt.Domain` contains entities and business rules without framework dependencies.
- `BookIt.Application` contains commands, handlers, the library facade, and repository/service contracts.
- `BookIt.Infrastructure` contains EF Core and PostgreSQL integration.
- `BookIt.Api` contains controllers, HTTP configuration, and dependency injection composition.

## Prerequisites

Install the .NET 10 SDK and Docker Desktop, then confirm they are available:

```powershell
dotnet --info
docker --version
docker compose version
```

## Run the complete local stack

Create the ignored local environment file and replace its database password and JWT signing key with local-only values. The signing key must contain at least 32 characters:

```powershell
Copy-Item .env.example .env
```

Build and start PostgreSQL, apply pending EF migrations, and start the API:

```powershell
docker compose -f docker-compose.local.yml up --build -d
docker compose -f docker-compose.local.yml ps
```

The one-shot `migrate` service must finish successfully before the API starts. It is safe to run the command again; EF only applies pending migrations.

The local endpoints are:

- Swagger UI: http://localhost:8080/docs
- OpenAPI document: http://localhost:8080/docs/v1/swagger.json
- Database-aware health check: http://localhost:8080/healthz

Verify the health endpoint from PowerShell:

```powershell
Invoke-RestMethod http://localhost:8080/healthz
```

Stop the stack while preserving PostgreSQL data:

```powershell
docker compose -f docker-compose.local.yml down
```

`docker compose -f docker-compose.local.yml down --volumes` also deletes the local database volume and all of its data.

The local Compose file uses the PostgreSQL 18-compatible mount at `/var/lib/postgresql` and a `postgres-data-v18` volume. If you previously ran this project with the old `/var/lib/postgresql/data` mount, the old volume is left untouched. For this initial local-only database, start with the new volume by running `docker compose -f docker-compose.local.yml up --build -d`; do not remove the old volume unless you have confirmed it contains no data you need.

The API container runs `dotnet watch`. Source code is mounted from the repository into the container, so changing C# files triggers the API to rebuild and restart automatically. `DOTNET_USE_POLLING_FILE_WATCHER` is enabled because bind-mounted file notifications can be unreliable on Windows. Container `bin` and `obj` directories use Docker volumes so Linux builds do not overwrite host build metadata.

## Run the API directly

Start only PostgreSQL:

```powershell
docker compose -f docker-compose.local.yml up -d postgres
```

Store the host connection string and JWT key outside the repository with .NET user-secrets. Use the same values as your ignored `.env` file:

```powershell
dotnet user-secrets set "ConnectionStrings:LibraryDatabase" "Host=localhost;Port=5432;Database=bookit;Username=bookit;Password=<your-local-password>" --project src/BookIt.Api
dotnet user-secrets set "Jwt:SigningKey" "<at-least-32-random-characters>" --project src/BookIt.Api
dotnet run --project src/BookIt.Api
```

Direct development uses http://localhost:5155. Swagger is enabled only when `ASPNETCORE_ENVIRONMENT` is `Development`.

## Entity Framework commands

Restore the repository-pinned EF CLI tool:

```powershell
dotnet tool restore
```

Inspect the configured context:

```powershell
dotnet ef dbcontext info --project src/BookIt.Infrastructure --startup-project src/BookIt.Api
```

Apply the included initial migration:

```powershell
dotnet ef database update --project src/BookIt.Infrastructure --startup-project src/BookIt.Api
```

For later schema changes, generate another migration before applying it:

```powershell
dotnet ef migrations add <MigrationName> --project src/BookIt.Infrastructure --startup-project src/BookIt.Api --output-dir Persistence/Migrations
```

The API process does not change the schema during startup. Local Compose delegates that job to its one-shot `migrate` service.

## Database schema

The `InitialLibrarySchema` migration creates:

- `books`, with a unique ISBN and checks that total copies are positive and available copies stay between zero and the total.
- `members`, with a unique normalized email and a password hash. Plain-text passwords are never stored.
- `loans`, with foreign keys to books and members, UTC timestamps, and checks for valid due and return dates.

All primary keys are PostgreSQL UUIDs. Refresh tokens are signed JWTs and are not stored in the database.

## Authentication endpoints

Registration requires a password of at least eight characters. Registration and login return an access token valid for one hour and a refresh token valid for a fixed 15 days.

```http
POST /api/auth/register
Content-Type: application/json

{
  "name": "Ada Lovelace",
  "email": "ada@example.com",
  "password": "password123"
}
```

```http
POST /api/auth/login
Content-Type: application/json

{
  "email": "ada@example.com",
  "password": "password123"
}
```

```http
POST /api/auth/refresh
Content-Type: application/json

{
  "refreshToken": "<refresh-token>"
}
```

Refresh returns a new one-hour access token together with the same refresh token and original expiration. Because refresh tokens are stateless in this version, they cannot be revoked before they expire. Use access tokens in Swagger's **Authorize** dialog or as `Authorization: Bearer <access-token>`.

## Build locally

```powershell
dotnet restore BookIt.slnx --configfile NuGet.Config
dotnet build BookIt.slnx --no-restore
dotnet test BookIt.slnx --no-restore
```

## Bootstrap command reference

These .NET CLI commands reproduce the repository structure and dependencies. They are documented so each generated file and package has a clear purpose.

| Command | Purpose |
| --- | --- |
| `dotnet new sln --name BookIt` | Create the solution container. |
| `dotnet new classlib --name BookIt.Domain --output src/BookIt.Domain` | Create the dependency-free domain project. |
| `dotnet new classlib --name BookIt.Application --output src/BookIt.Application` | Create the application use-case project. |
| `dotnet new classlib --name BookIt.Infrastructure --output src/BookIt.Infrastructure` | Create the persistence and external-service project. |
| `dotnet new webapi --name BookIt.Api --output src/BookIt.Api --use-controllers --no-openapi` | Create the controller-based API; Swagger is added explicitly. |
| `dotnet new xunit --name BookIt.Tests --output tests/BookIt.Tests` | Create the domain, application, and JWT unit-test project. |
| `dotnet sln BookIt.slnx add ...` | Register all four projects in the solution. |
| `dotnet add ... reference ...` | Establish the inward project dependency direction. |
| `dotnet add src/BookIt.Infrastructure package Npgsql.EntityFrameworkCore.PostgreSQL --version 10.0.3` | Add the PostgreSQL EF Core provider. |
| `dotnet add src/BookIt.Infrastructure package Microsoft.Extensions.Identity.Core --version 10.0.12` | Add ASP.NET Core's standard password hasher without the full Identity schema. |
| `dotnet add src/BookIt.Infrastructure package System.IdentityModel.Tokens.Jwt --version 8.16.0` | Create and validate signed access and refresh JWTs. |
| `dotnet add src/BookIt.Api package Microsoft.AspNetCore.Authentication.JwtBearer --version 10.0.12` | Validate bearer access tokens for protected API endpoints. |
| `dotnet add src/BookIt.Api package Microsoft.EntityFrameworkCore.Design --version 10.0.12` | Enable EF design-time commands through the startup project. |
| `dotnet add src/BookIt.Api package Microsoft.Extensions.Diagnostics.HealthChecks.EntityFrameworkCore --version 10.0.12` | Add the `DbContext` connectivity health check. |
| `dotnet add src/BookIt.Api package Swashbuckle.AspNetCore --version 10.2.3` | Generate OpenAPI and host Swagger UI. |
| `dotnet add tests/BookIt.Tests package Microsoft.NET.Test.Sdk --version 18.0.1` | Add the .NET test host. |
| `dotnet add tests/BookIt.Tests package xunit --version 2.9.3` | Add the xUnit test framework. |
| `dotnet add tests/BookIt.Tests package xunit.runner.visualstudio --version 3.1.5` | Let `dotnet test` discover and execute xUnit tests. |
| `dotnet new tool-manifest` | Create a repository-local .NET tool manifest. |
| `dotnet tool install dotnet-ef --version 10.0.12` | Pin the EF CLI version used by the repository. |
| `dotnet user-secrets init --project src/BookIt.Api` | Enable untracked secrets for direct host development. |
| `dotnet ef migrations add InitialLibrarySchema --project src/BookIt.Infrastructure --startup-project src/BookIt.Api --output-dir Persistence/Migrations` | Generate the initial books, members, and loans schema. |
| `dotnet ef database update --project src/BookIt.Infrastructure --startup-project src/BookIt.Api` | Apply pending migrations to the configured PostgreSQL database. |
