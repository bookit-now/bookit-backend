# AGENTS.md

## Project overview

This repository is intended to become a .NET API for a small book-lending library. Keep this context in mind when making future changes, but do not expand the scope beyond the user's current request.

The core domain is:

- `Book`: id, title, author, ISBN, total copies, available copies
- `Member`: id, name, email
- `Loan`: id, book id, member id, borrowed at, due date, and nullable returned at

The API should support these use cases:

- Commands: register a member, add a book, borrow a book, and return a book
- Queries: list available books with optional author/title filters, get a member's active loans, and get overdue loans

Borrowing is a meaningful domain operation. The domain must protect rules such as requiring an available copy and limiting how many active loans a member may have. Keep these rules in the domain/application boundary rather than hiding them in controllers or persistence code.

## Planned API expansion

The API will eventually be consumed by a Flutter mobile application, so new endpoints should be designed as a stable, documented client-facing API. Use consistent resource names, HTTP status codes, error responses, validation behavior, date/time formats, and JSON naming. Avoid leaking EF Core entities or internal implementation details into responses.

The following features are planned as additional practice using the same architecture and patterns:

- `Auth`: member registration and JWT login. Protected endpoints should use ASP.NET Core authentication with `AddAuthentication().AddJwtBearer()` and authorization policies or `[Authorize]`.
- `Validation`: FluentValidation validators for commands and queries where appropriate. When a mediator is used, validation should run through a pipeline behavior so handlers receive valid requests consistently.
- `Pagination and filtering`: extend the available-books query with pagination and filters. Keep pagination parameters explicit and use `IQueryable`, `Skip`, and `Take` in the infrastructure query implementation without exposing persistence concerns to the API.
- `Reservations`: allow a member to reserve an unavailable book and receive it when a copy is returned. This introduces a new entity and new invariants while following the existing CQRS structure.

Cross-cutting concerns should be handled in shared infrastructure or pipeline components rather than duplicated in individual handlers:

- MediatR pipeline behaviors for logging, validation, and transactions
- Global exception handling middleware that maps domain exceptions such as `NoCopiesAvailableException` to appropriate HTTP status codes and consistent error bodies
- Structured logging with Serilog
- Health checks for the API and database
- Swagger/OpenAPI documentation and testing support for every public endpoint

Swagger/OpenAPI is part of the API contract, especially because the Flutter client will depend on the documented request and response shapes. Keep schemas, authentication requirements, status codes, validation errors, and example payloads accurate as endpoints evolve.

Later, consider fines for overdue loans. Fines may be calculated or recorded by a background process using a hosted service or scheduled job. Keep that work outside request handlers, make it safe to retry, and preserve clear boundaries so a future cron-based deployment can invoke the same application use case.

## Intended architecture

Use a Clean Architecture layout with four projects and dependencies pointing inward:

```text
Api -> Infrastructure -> Application -> Domain
                         \-> Domain
```

The projects have these responsibilities:

- `Domain`: entities, value objects if needed, domain rules, and domain errors. It must have zero framework or infrastructure dependencies.
- `Application`: commands, queries, handlers, use-case contracts, interfaces such as `IBookRepository`, and the application-facing `ILibraryFacade`. It depends only on `Domain`.
- `Infrastructure`: EF Core `DbContext`, repository implementations, database configuration, migrations, and PostgreSQL integration.
- `Api`: HTTP controllers/endpoints, request/response models, dependency injection, configuration, and `Program.cs`.

Do not make domain entities depend on EF Core annotations or HTTP concerns. Keep repository abstractions in `Application` and their implementations in `Infrastructure`.

## CQRS and application entry point

Represent each use case as a command or query plus a handler. Commands change state; queries read state. Controllers should call `ILibraryFacade` and should not coordinate handlers, repositories, or nested service chains directly.

The facade may delegate to a mediator or a small hand-rolled dispatcher. MediatR is an option, but avoid introducing a dependency solely for ceremony; the separation between commands, queries, and handlers is the important part.

Keep Law of Demeter in mind: controllers talk to the facade, and each component talks to its immediate collaborators. Avoid chains such as `facade.GetMediator().GetHandler().Repository.Save()`.

## Persistence and running locally

Use EF Core with PostgreSQL. Local development is expected to run through Docker Compose, including the API dependencies and a PostgreSQL container. Deployment Compose configuration should support a separately managed PostgreSQL instance, so it must not assume that a database container is always present.

Keep connection strings and environment-specific settings in configuration or environment variables. Never commit credentials or machine-specific secrets.

## Working conventions

- Preserve the dependency direction and project responsibilities above when adding code.
- Put business invariants in a testable domain/application location, not in controllers.
- Keep API models separate from persistence entities when that separation is useful for the use case.
- Prefer focused changes that match the current user request; do not implement the whole roadmap unless asked.
- Add or update meaningful tests for business rules and use cases when test infrastructure exists.
- Before handing off changes, run the repository's available build, test, formatting, and migration checks that apply to the files changed.
- Update the README when setup, Docker Compose usage, endpoints, or developer workflow changes.

## Current repository state

The repository contains the .NET 10 Clean Architecture solution, PostgreSQL/EF Core wiring, the initial `books`, `members`, and `loans` migration, domain entities, and stateless JWT registration/login/refresh endpoints. Access tokens expire after one hour; refresh tokens have a fixed 15-day lifetime and are not persisted or revocable. Swagger is available at `/docs`, `/healthz` checks PostgreSQL, and local Compose applies migrations before starting the watched API. The remaining library commands and queries have not been implemented yet.
