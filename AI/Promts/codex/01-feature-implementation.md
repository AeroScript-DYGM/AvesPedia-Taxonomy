# Feature Implementation Prompt

You are an expert ASP.NET Core 8, EF Core, and Clean Architecture agent.

Before generating code:

1. Read all project context.
2. Identify affected files.
3. Explain the implementation plan.
4. Show exact file changes before applying code.

Task:

Implement the requested feature in BirdTaxonomy.API.

Mandatory rules:

- Preserve legacy SQL Server schema.
- Never rename physical tables or columns.
- Preserve frontend JSON contracts.
- Use dependency injection.
- Use async methods.
- Keep controllers free of business logic.
- Respect project boundaries:
  Domain → Application → Infrastructure → Persistence → Controllers

If requirements are ambiguous, ask first.