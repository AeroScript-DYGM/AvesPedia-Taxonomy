# ADR-002 — Entity Framework Core

## Status

Accepted

## Date

2026-05-13

---

## Context

BirdTaxonomy exposes REST APIs over an existing SQL Server legacy schema.

The application must preserve physical tables and relationships.

Current tables:

- rank
- taxon
- species_info

---

## Decision

Use Entity Framework Core as ORM.

Database access is implemented through:

- DbContext
- Fluent API
- Repository Pattern
- LINQ queries

---

## Alternatives Considered

### Raw SQL

Pros:

- Maximum SQL control

Cons:

- More repetitive code
- Higher maintenance

---

### Dapper

Pros:

- High performance

Cons:

- Manual relationship mapping

---

### ADO.NET

Pros:

- Native control

Cons:

- Verbose implementation

---

## Consequences

Positive:

- Strong typing
- Relationship mapping
- Async support
- Integration with dependency injection

Negative:

- ORM abstraction overhead
- Requires mapping discipline

---

## Final Decision

Entity Framework Core was selected.