# Prompt Engineering for BirdTaxonomy API

## Overview

Prompt engineering is the systematic process of designing, refining, and validating prompts that guide large language models (LLMs) to generate code, documentation, and architectural decisions for the BirdTaxonomy API. This document establishes standards, patterns, and best practices for all AI-assisted development activities within this project.

## Definition

Prompt engineering encompasses:

- **Specification**: Clear articulation of desired outcomes, constraints, and success criteria
- **Context provision**: Supplying relevant codebase knowledge, architectural patterns, and domain constraints
- **Instruction precision**: Unambiguous direction regarding format, style, and validation rules
- **Iteration**: Refinement based on output quality and project requirements
- **Validation**: Verification that generated artifacts meet functional and non-functional requirements

## Why Prompt Engineering Matters for BirdTaxonomy

### Legacy System Complexity

BirdTaxonomy interfaces with a legacy SQL Server taxonomy database containing normalized structures across the `rank`, `taxon`, and `species_info` tables. AI models lack inherent knowledge of these domain-specific schemas and relationships. Well-engineered prompts bridge this gap by:

- Establishing explicit table relationships and constraints
- Defining cardinality rules
- Clarifying business logic constraints
- Preventing generation of invalid SQL patterns

### Clean Architecture Adherence

The project maintains strict separation across Controllers, Application, Domain, Infrastructure, and Persistence layers. Generic prompts often violate these boundaries by collapsing layers or mixing responsibilities. Engineered prompts enforce:

- Correct DTO usage at layer boundaries
- Repository pattern compliance
- Dependency injection configuration
- Entity Framework Core best practices

### API Contract Stability

REST endpoint contracts govern downstream client consumption. Prompt engineering ensures:

- Consistent HTTP method semantics (GET, POST, PUT, DELETE)
- Proper HTTP status code usage (200, 201, 400, 404, 500)
- Swagger/OpenAPI specification alignment
- Request/response schema consistency

### Portfolio Quality Standards

Documentation and code generated with AI assistance must meet academic and professional standards. Strategic prompting ensures:

- Clear variable and method naming
- Comprehensive XML documentation comments
- Proper async/await patterns in C#
- Error handling completeness

## Prompt Design Principles

### 1. Specificity Over Generality

**Weak**: "Create an API endpoint for bird data."

**Strong**: "Create a GET endpoint at `api/birds/{taxonId}` that returns a BirdDTO containing taxonomic rank, common name, scientific name, and species information. Use the Repository pattern to abstract database access through `ITaxonRepository`. Return HTTP 200 on success, 404 if taxonId does not exist."

### 2. Constraint Declaration

Always explicitly state constraints that should guide generation:

- Layer boundaries and responsibilities
- Database schema limitations
- Existing interfaces that must be implemented
- Naming conventions in use
- Async/await requirements
- Null reference handling strategy

### 3. Example-Driven Specificity

Provide concrete examples from the existing codebase:

```
Follow this pattern for DTOs:
public class BirdDTO
{
    public int Id { get; set; }
    public string ScientificName { get; set; }
    public string CommonName { get; set; }
    public string RankName { get; set; }
}
```

### 4. Success Criteria Definition

Define measurable outputs:

- Must not generate any SQL queries (only Repository calls)
- Must include null reference handling for optional fields
- Must include XML documentation with `<summary>`, `<param>`, and `<returns>` tags
- Must register dependency injection in Program.cs with explicit interface binding

### 5. Role Clarity

Specify the AI's role and limitations:

- "You are a C# ASP.NET Core API developer with expertise in Entity Framework Core"
- "Do not generate database migration files"
- "Do not suggest architectural changes outside Clean Architecture patterns"
- "Flag any requirements that conflict with existing constraints"

## Prompt Examples for GitHub Copilot

Copilot operates within the IDE context, making it effective for incremental code generation and localized refactoring.

### Example 1: Adding a Repository Method

**Prompt**:
```
Add a method to ITaxonRepository to retrieve all species within a given rank.
The method signature should be:
Task<IEnumerable<TaxonDto>> GetSpeciesByRankAsync(string rankName)

Requirements:
- Use Entity Framework Core with async/await
- Filter the taxon table where rank.name matches rankName
- Return an empty IEnumerable if no matches found (no null)
- Include XML documentation
- Map domain entities to TaxonDto using a mapping pattern

Follow the existing pattern in this repository.
```

**Success Criteria**:
- Method exists on interface
- Implementation uses DbSet<Taxon> and LINQ
- No synchronous blocking calls
- Proper null coalescing for empty results

### Example 2: Creating a Service Class

**Prompt**:
```
Create a BirdSearchService in the Application layer that:
1. Depends on ITaxonRepository (injected via constructor)
2. Has a method: Task<IEnumerable<BirdSearchResultDto>> SearchByScientificNameAsync(string pattern)
3. Uses the repository to query for partial matches
4. Maps results to BirdSearchResultDto
5. Throws a custom SearchException if the pattern is null or whitespace
6. Includes comprehensive XML documentation

The class should be registered in Program.cs with:
services.AddScoped<IBirdSearchService, BirdSearchService>()

Place this in the Application/Services directory.
```

**Success Criteria**:
- Class implements IBirdSearchService interface
- Constructor dependency injection of repository
- Pattern validation before repository call
- DTO mapping logic included
- Exception handling documented

### Example 3: Expanding Swagger Documentation

**Prompt**:
```
Add Swagger annotations to the GetBirdById(int id) endpoint controller method.

Requirements:
- Add [Produces("application/json")] attribute
- Add [ProduceResponseType(typeof(BirdDto), StatusCodes.Status200OK)]
- Add [ProduceResponseType(StatusCodes.Status404NotFound)]
- Add [ProduceResponseType(StatusCodes.Status500InternalServerError)]
- Add XML documentation with <summary>, <param>, <returns>, and <exception> tags
- Include a clear description of what the endpoint does and possible failure modes

Use this endpoint as reference for patterns:
[HttpGet("{id}")]
public async Task<ActionResult<BirdDto>> GetBirdById(int id)
```

**Success Criteria**:
- All response types documented
- Status codes match actual implementation
- XML documentation complete
- Swagger UI reflects all documented behaviors

## Prompt Examples for OpenAI Codex

Codex operates in a notebook or chat context, making it suitable for architecture discussion, documentation generation, and cross-cutting concerns.

### Example 1: Architecture Documentation

**Prompt**:
```
Write a detailed explanation of how the BirdTaxonomy API implements the Repository pattern.

Include:
1. The purpose of the repository pattern in this context
2. The ITaxonRepository interface contract
3. How the repository abstracts SQL Server queries
4. How repositories are registered in dependency injection
5. A concrete example of a repository method and its usage flow
6. Benefits specific to maintaining a legacy taxonomy database

Target audience: junior developers and architects reviewing the system.
Format: Professional Markdown documentation.
```

**Success Criteria**:
- Clear explanation of abstraction benefits
- References to actual project structure
- Diagram or flowchart showing data flow
- Concrete code references (not pseudocode)

### Example 2: Database Schema Documentation

**Prompt**:
```
Create comprehensive documentation for the BirdTaxonomy database schema.

Tables to document:
1. rank (columns: id, name, description)
2. taxon (columns: id, name, rank_id, parent_id)
3. species_info (columns: id, taxon_id, common_name, description)

For each table, provide:
1. Purpose in the taxonomy system
2. Column descriptions and data types
3. Primary and foreign key relationships
4. Cardinality constraints
5. Indexed columns and query patterns
6. Historical constraints or quirks (legacy system notes)

Format: A single Markdown document with tables and relationship diagrams.
Target audience: Backend developers writing queries and Entity Framework mappings.
```

**Success Criteria**:
- All relationships clearly documented
- Example queries showing typical usage
- Legacy system notes flagged for new developers
- Scalability considerations noted

### Example 3: API Contract Specification

**Prompt**:
```
Generate OpenAPI/Swagger specification documentation for the following endpoints:

Endpoints:
1. GET /api/birds - List all birds with pagination
2. GET /api/birds/{id} - Retrieve specific bird details
3. POST /api/birds - Create a new bird taxonomy entry
4. PUT /api/birds/{id} - Update bird information
5. DELETE /api/birds/{id} - Remove bird from taxonomy

For each endpoint:
1. Provide complete request schema (parameters, body)
2. Provide response schemas for success and error cases
3. Document required vs optional fields
4. Define HTTP status codes and their meanings
5. Note any query string filtering options
6. Specify authentication requirements (if applicable)

Format: Markdown with code blocks showing example requests/responses.
Target audience: Frontend developers consuming the API.
```

**Success Criteria**:
- All endpoints fully specified
- Example payloads are valid JSON
- Status codes align with REST conventions
- Error responses documented

## Prompt Versioning Strategy

### Version Tracking

Maintain a `Docs/AI/prompt-library.md` file tracking all approved prompts:

| Prompt ID | Purpose | Version | Last Updated | Status | Notes |
|-----------|---------|---------|--------------|--------|-------|
| PROM-001 | Repository Method Generation | 2.1 | 2025-03-15 | Approved | Added async requirement |
| PROM-002 | Service Class Creation | 1.8 | 2025-02-28 | Approved | Removed sync alternative |
| PROM-003 | Controller Endpoint | 3.0 | 2025-04-10 | Approved | Added pagination pattern |

### Versioning Rationale

- **Major version** (e.g., 1.0 → 2.0): Significant architecture changes or paradigm shifts
- **Minor version** (e.g., 1.0 → 1.1): New constraints, additional examples, or clarified requirements
- **Patch version** (e.g., 1.0 → 1.0.1): Grammar fixes, typo corrections, formatting improvements

### Prompt Git History

Store approved prompts in `Docs/AI/prompt-versions/` with commit messages indicating changes:

```
commit: Update PROM-002 to require explicit DI registration
- Added requirement for Program.cs configuration
- Clarified interface segregation expectations
- Version bumped to 1.9
```

## Prompt Validation Rules

### Pre-Submission Validation

Before using a prompt with AI models, verify:

1. **Completeness**: All required constraints are stated explicitly
2. **Clarity**: No ambiguous terminology or undefined acronyms
3. **Consistency**: Constraints don't contradict each other
4. **Feasibility**: Requested output is realistically achievable
5. **Specificity**: No placeholder language like "etc." or "and so on"

### Output Validation

After AI generation, validate outputs against:

| Criterion | Validation Method | Pass/Fail |
|-----------|-------------------|-----------|
| Layer compliance | Code inspection against architecture diagram | Binary |
| Compilation | `dotnet build` without errors or warnings | Binary |
| API contract | Swagger generation and schema validation | Binary |
| Documentation | XML doc completeness check | Binary |
| Test coverage | Unit test generation for new methods | Threshold-based |
| Security | Code review for SQL injection, auth bypasses | Binary |

## Prompt Anti-Patterns

### Anti-Pattern 1: Over-Specification Leading to Rigidity

**Problem**: Prompts that specify every detail often produce brittle, non-idiomatic code.

**Example**:
```
Write a method that does this:
1. Open connection
2. Execute query
3. Read first row
4. Close connection
5. Return row
```

**Issue**: Prescribes procedural steps rather than intent, ignoring async patterns and Entity Framework.

**Correction**:
```
Write an async method that retrieves a single taxon by ID.
Use Entity Framework Core's FirstOrDefaultAsync for efficient single-row retrieval.
Return null if not found; do not throw exceptions.
```

### Anti-Pattern 2: Insufficient Context

**Problem**: Prompts lacking project context generate code that conflicts with existing patterns.

**Example**:
```
Write a method to get birds by name.
```

**Issue**: No reference to repository pattern, layer structure, or DTO usage.

**Correction**:
```
Implement GetBirdsByCommonNameAsync(string pattern) in the TaxonRepository.
Follow the existing pattern in GetBirdsByScientificNameAsync.
Return IEnumerable<TaxonDto> (mapped from Taxon domain entities).
Use LINQ with EF Core's Like extension for pattern matching.
```

### Anti-Pattern 3: Ambiguous Success Criteria

**Problem**: Unmeasurable success criteria prevent validation.

**Example**:
```
Write good error handling.
Generate clean code.
Make it performant.
```

**Issue**: "Good," "clean," and "performant" are subjective without concrete thresholds.

**Correction**:
```
Error handling success criteria:
- All ArgumentNull exceptions for required parameters
- Custom TaxonException for domain-level failures
- HttpStatusCodeException for API-level errors
- Each catch block logs using ILogger interface

Performance criteria:
- No N+1 queries (verified with EF Core logging)
- Single database round-trip for single-entity retrieval
- Pagination limit enforced at query level (max 100 results)
```

### Anti-Pattern 4: Mixing Architecture Layers

**Problem**: Prompts that don't respect layer boundaries generate code that violates Clean Architecture.

**Example**:
```
Write a controller that loads data from the database and returns it to the user.
```

**Issue**: Violates Clean Architecture by placing database access in the controller layer.

**Correction**:
```
Write a controller method that:
1. Accepts user input via HTTP request
2. Calls an application service via dependency injection
3. Returns the result as HTTP response

The controller is responsible only for HTTP semantics, not business logic or data access.
Place database queries in the Infrastructure/Persistence layer via repositories.
```

### Anti-Pattern 5: Generic Boilerplate Instruction

**Problem**: Using the same prompt across different projects or contexts ignores domain-specific knowledge.

**Example**:
```
Generate a CRUD controller for a database table.
```

**Issue**: Doesn't account for BirdTaxonomy's specific schemas, constraints, or API contracts.

**Correction**:
```
Generate a controller for managing Taxon entities in the BirdTaxonomy API.

Endpoints required:
- GET /api/taxa - Return paginated list of taxa with filters by rank
- GET /api/taxa/{id} - Return single taxon with all species information
- POST /api/taxa - Create new taxon (requires scientific name, rank)
- PUT /api/taxa/{id} - Update taxon (name, rank)

Constraints:
- Parent taxon must exist if parent_id is provided
- Rank must be valid (reference rank table)
- No duplicate scientific names within same rank
- Return appropriate HTTP status codes (201 for creation, 400 for validation, 404 for missing)
```

## Best Practices

### Practice 1: Incremental Prompting

Rather than requesting an entire system at once, break work into coherent increments:

**Single Prompt Sequence**:
```
1. Prompt: "Create the ITaxonRepository interface with the contracts needed for bird retrieval"
   → Review and validate against domain model

2. Prompt: "Implement TaxonRepository with the methods defined in ITaxonRepository"
   → Verify Entity Framework Core patterns and async usage

3. Prompt: "Create the BirdService application layer class using the repository"
   → Check dependency injection and business logic

4. Prompt: "Generate the BirdController with endpoints using the service"
   → Validate HTTP semantics and API contract

5. Prompt: "Add Swagger documentation and response type attributes"
   → Confirm OpenAPI specification completeness
```

Benefits: Each step validates before proceeding, reducing rework and improving quality.

### Practice 2: Prompt Chaining with Context Preservation

When one prompt builds on another, carry context forward:

**Prompt 1**:
```
Create a DTO named BirdSearchResultDto with fields: Id, ScientificName, CommonName, RankName.
Include data annotations for validation.
```

**Prompt 2** (references Prompt 1):
```
Building on the BirdSearchResultDto from the previous request:
Create a search service that returns IEnumerable<BirdSearchResultDto>.
The service should use the TaxonRepository to find matches by scientific or common name pattern.
Include case-insensitive matching.
```

Benefits: Maintains consistency and prevents contradictory generation across related components.

### Practice 3: Embedding Project Standards

Include project-specific standards in every prompt:

```
BirdTaxonomy API Standards:
- Async/await for all I/O operations (no blocking calls)
- Dependency injection via constructor parameters
- DTOs at API boundaries, domain entities in layers
- Null-reference safety: use null coalescing and null-conditional operators
- XML documentation on all public members
- Custom exceptions: TaxonException, SpeciesException
- Database queries only in Infrastructure layer via repositories
```

Benefits: Ensures consistency without repeating standards in every prompt.

### Practice 4: Validation Checkpoints

After each prompt-generated artifact, establish a validation checkpoint:

1. **Syntax validation**: Code compiles without errors
2. **Pattern validation**: Follows established architectural patterns
3. **Contract validation**: Matches API specification
4. **Documentation validation**: All public members documented
5. **Security validation**: No SQL injection or authentication bypasses
6. **Performance validation**: Efficient database access, no N+1 queries

Only proceed to next prompt after checkpoint passes.

### Practice 5: Prompt Documentation

Document the reasoning behind complex prompts:

```markdown
## Prompt: Generate Pagination for List Endpoints

### Rationale
List endpoints returning hundreds of taxa create performance and network issues.
Pagination is necessary but introduces complexity in query specification and response structure.

### Constraints
- Use skip/take pattern in Entity Framework Core
- Default page size: 20, maximum: 100
- Client must request explicit page number (no cursor-based pagination)
- Response includes total count for client-side pagination UI

### Related Standards
- See Docs/API/pagination-standard.md for contract specification
- Follow existing pagination in GetBirdsAsync endpoint

### Generated Artifacts
- Updated TaxonRepository with paginated queries
- Updated BirdDto with PaginationMetadata
- Controller method updates with [FromQuery] parameters
```

Benefits: Future developers understand design decisions and can refine prompts systematically.

### Practice 6: Prompt Testing

Before approving a prompt for production use, test it with different models:

| Model | Result | Issues | Version |
|-------|--------|--------|---------|
| GitHub Copilot | Pass | None | 2.0 |
| OpenAI Codex | Pass | Inconsistent DI registration format | 2.0 |
| Claude 3.5 Sonnet | Pass | Over-documented XML comments | 2.0 |

Maintain a test report in `Docs/AI/prompt-test-results.md`.

## Conclusion

Systematic prompt engineering transforms AI assistance from unpredictable magic to reliable tooling. By establishing clear constraints, validating outputs rigorously, and iterating based on results, the BirdTaxonomy API team leverages AI models as professional development partners rather than experimental features.

All prompts must be versioned, validated, and documented. Every generated artifact passes through human review before merge. This discipline maintains code quality, architectural integrity, and project momentum.