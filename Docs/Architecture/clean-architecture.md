# BirdTaxonomy — Clean Architecture

## Architectural Style

BirdTaxonomy follows Clean Architecture principles combined with
Layered Architecture.

The objective is to maintain:

- Separation of concerns
- Testability
- Maintainability
- Scalability
- Explicit dependency flow

---

## Dependency Rule

All dependencies must point toward the center.

Outer layers depend on inner layers.

Inner layers never depend on infrastructure details.

Flow:

Infrastructure
→ Application
→ Domain

Never:

Domain
→ Infrastructure

---

## Current Logical Layers

Although the solution is currently implemented as a single project,
architectural boundaries are preserved logically.

---

## 1. Presentation Layer

Folder:

```text id="6ck1ii"
Controllers/