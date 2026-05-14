PROJECT CONTEXT

Project name:

Bird Taxonomy Backend System

Current state:

The repository currently contains a single ASP.NET Core Web API project named `BirdTaxonomy.API`.
Until the multi-project solution is split physically, architecture boundaries must still be preserved logically
through folders, namespaces, interfaces, services, repositories, and explicit dependency flow.

Purpose:

This project exposes REST APIs over an existing scientific bird taxonomy database.

Technology stack:

- C#
- ASP.NET Core Web API
- Entity Framework Core
- SQL Server LocalDB
- Visual Studio 2022

Architecture:

- Clean Architecture
- Data Driven Architecture
- Layered Architecture

Logical layers inside the current project:

- `Domain`
- `Application`
- `Infrastructure`
- `Persistence`
- `Shared`
- `Controllers`

Target future split:

- Aves.Api
- Aves.Application
- Aves.Domain
- Aves.Infrastructure
- Aves.Persistence
- Aves.Shared

Domain:

This project currently integrates against an existing SQL Server LocalDB database named `Aves`.
The ORM and API must adapt to the existing schema instead of forcing a new physical taxonomy schema.

Current physical tables:

- `rank`
- `taxon`
- `species_info`

Meaning of current entities:

- `rank` stores taxonomy rank catalogs.
- `taxon` stores named taxa and references `rank`.
- `species_info` stores species-level details associated with a `taxon`.

Database rules:

rank 1:N taxon
taxon 1:0..1 species_info

Domain entities:

- Rank
- Taxon
- SpeciesInfo

Design principles:

- SOLID
- DRY
- Single Responsibility
- Open Closed
- Dependency Inversion
- Separation of Concerns

Patterns:

- Repository Pattern
- Unit of Work
- DTO Pattern
- Dependency Injection
- Fluent API

Controller rules:

Controllers must:

- receive requests
- validate input
- call services
- return responses

Controllers must never contain business logic.

Application rules:

Services contain use cases and orchestration logic.

Infrastructure rules:

Repositories handle persistence.

Persistence rules:

- Entity Framework Core
- Fluent API for all relationships
- SQL Server LocalDB
- Historical note: the first implementation draft used SQLite temporarily before alignment to SQL Server LocalDB.
- Existing database name: `Aves`
- Explicit delete behavior configuration

Communication resilience:

- If SQL Server LocalDB communication fails, the API contract and documentation must reflect HTTP 500.

Current API surface:

- `GET /api/taxonomia/rangos`
- `GET /api/taxonomia/taxones`
- `GET /api/taxonomia/taxones/{id}`
- `POST /api/taxonomia/taxones`
- `PUT /api/taxonomia/taxones/{id}`
- `DELETE /api/taxonomia/taxones/{id}`

Historical note:

- The API started as read-only and was later extended to support write operations over the existing legacy schema.

Coding standards:

- PascalCase
- Async methods
- Nullable references
- Strong typing
- Constructor dependency injection

AI behavior:

Always preserve existing database contracts.

Never rename physical tables without explicit user request.

Never flatten relationships already present in the database.

Never break project boundaries.

If uncertain, ask before generating code.
