# ADR-003 — Production Hosting on MonsterASP.NET

## Status

Accepted

## Date

2026-05-13

---

## Context

BirdTaxonomy requires public deployment of an ASP.NET Core Web API
with SQL Server compatibility.

The project must be deployable from Visual Studio publish profiles.

---

## Decision

Use MonsterASP.NET as production hosting provider.

Production environment includes:

- IIS hosting
- SQL Server database
- Publish profile deployment

---

## Alternatives Considered

### Azure App Service

Pros:

- Native Microsoft ecosystem
- High scalability

Cons:

- Higher operational cost

---

### Self-hosted VPS

Pros:

- Full server control

Cons:

- Infrastructure management overhead

---

### Docker-only deployment

Pros:

- Portability

Cons:

- Requires additional orchestration

---

## Consequences

Positive:

- Native ASP.NET support
- SQL Server support
- Easy Visual Studio deployment

Negative:

- Shared hosting limitations
- Less infrastructure customization

---

## Final Decision

MonsterASP.NET was selected as the production hosting provider.