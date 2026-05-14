# ADR-001 — Clean Architecture

## Status

Accepted

## Date

2026-05-13

---

## Context

BirdTaxonomy started as a single ASP.NET Core Web API project.

Although the repository currently contains only one physical project,
the system is expected to evolve into a multi-project solution.

The application integrates with a legacy scientific taxonomy database
and requires long-term maintainability.

---

## Decision

Use Clean Architecture with logical separation through folders,
namespaces, interfaces, services, repositories, and explicit dependency flow.

Logical layers:

- Controllers
- Application
- Domain
- Infrastructure
- Persistence

---

## Alternatives Considered

### Traditional MVC

Pros:

- Faster initial development

Cons:

- Business logic tends to leak into controllers

---

### N-Tier without domain boundaries

Pros:

- Familiar structure

Cons:

- Weak separation of business rules

---

### Monolithic procedural structure

Pros:

- Fast prototyping

Cons:

- Difficult testing
- Difficult scaling

---

## Consequences

Positive:

- Separation of concerns
- Easier unit testing
- Future migration to multi-project solution
- Easier AI-assisted development

Negative:

- More initial complexity
- More interfaces and abstractions

---

## Final Decision

Clean Architecture was selected.