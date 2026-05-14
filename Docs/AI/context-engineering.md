# Context Engineering for BirdTaxonomy API

## Overview

Context engineering is the discipline of structuring, maintaining, and evolving the contextual information provided to AI systems to ensure accurate, consistent, and architecturally-sound code generation. For BirdTaxonomy API, context engineering bridges the gap between legacy database complexity, clean architecture constraints, and AI model capabilities. This document establishes context boundaries, preservation strategies, and drift mitigation.

## Definition

Context engineering encompasses:

- **Context Specification**: Defining what information AI models must know about the project
- **Context Boundary Definition**: Establishing what is in-scope vs. out-of-scope for AI assistance
- **Context Persistence**: Strategies for maintaining consistent context across multiple AI interactions
- **Context Validation**: Verification that context remains accurate and non-contradictory
- **Context Evolution**: Updating context as the system grows and constraints change

## Why Context is Critical for Legacy Database Systems

### Legacy Database Impedance Mismatch

BirdTaxonomy wraps a legacy SQL Server taxonomy database. This introduces impedance mismatch:

- **Table Organization**: The legacy schema uses normalized tables (`rank`, `taxon`, `species_info`) with specific cardinality rules
- **Query Patterns**: Efficient queries require specific join patterns and index awareness
- **Historical Constraints**: Tables may have quirks, deprecated columns, or unconventional relationships
- **Business Rules Embedded in Schema**: Taxonomy relationships express real-world constraints

Without explicit context, AI models generate code assuming modern ORM-friendly schemas, resulting in:

- N+1 query problems
- Inefficient joins across multiple tables
- Missing null-safety for optional relationships
- Incorrect cardinality assumptions

### Multi-Layer Architecture Requires Boundary Clarity

Clean Architecture mandates strict layer separation:

```
Controllers (HTTP semantics)
    ↓
Application (business logic, services)
    ↓
Domain (entities, specifications)
    ↓
Infrastructure (repositories, EF Core mappings)
    ↓
Persistence (database access)
```

Without explicit context about layer responsibilities, AI models:

- Generate SQL directly in controllers
- Mix business logic across layers
- Violate dependency inversion principle
- Create tight coupling between layers

### API Contract Stability at Scale

REST endpoints form a public contract with clients. Context must establish:

- Standard status code semantics (200, 201, 400, 404, 500)
- Pagination and filtering conventions
- Error response format
- Authentication/authorization expectations
- Versioning strategy

Changes to these conventions ripple through all endpoints, making context definition critical for consistency.

### Knowledge Preservation Across Team Changes

As team composition changes, context captures institutional knowledge:

- Why specific patterns were chosen
- Known limitations of the legacy database
- Performance tuning decisions
- Historical decisions and alternatives considered

Without documented context, new team members and AI models alike make suboptimal decisions.

## Project Context Boundaries

### In-Scope Context: What AI Models Must Know

#### 1. Database Schema

**Required Context**:

| Table | Purpose | Key Constraints |
|-------|---------|-----------------|
| `rank` | Hierarchical taxonomic ranks (Kingdom, Phylum, Class, Order, Family, Genus, Species) | Immutable reference data; may contain legacy ranks no longer in use |
| `taxon` | Biological taxa entries with hierarchical relationships | Self-referential via `parent_id`; unique constraints on (name, rank); orphaned parent_ids possible in legacy data |
| `species_info` | Extended information for species-level taxa | Nullable `common_name`, `description`; one-to-one with species-rank taxa |

**Relationship Mappings**:

```
rank (1) ←→ (many) taxon (via rank_id)
taxon (1) ←→ (many) taxon (via parent_id - self-referential)
taxon (1) ←→ (0..1) species_info (via taxon_id)
```

**Legacy Constraints**:

- `species_info` may have records for non-species taxa (data quality issue)
- `parent_id` may reference non-existent taxa (orphaned records)
- `rank` table contains unused legacy ranks still in database for historical compatibility

**Cardinality Rules**:

- One rank can have many taxa
- One taxon can have many child taxa (hierarchy)
- Species-level taxa have at most one species_info record
- Non-species taxa should not have species_info (but may due to legacy data)

#### 2. Entity Framework Core Mappings

**Required Context**:

- Entity classes: `Rank`, `Taxon`, `SpeciesInfo`
- DbSet properties in `BirdTaxonomyDbContext`
- Navigation properties reflecting one-to-many and one-to-one relationships
- Fluent API configurations for legacy schema quirks

**Example Mapping Constraints**:

```
Taxon entity must:
- Have navigation property: ICollection<Taxon> Children (from parent_id)
- Have navigation property: Taxon Parent (to parent_id)
- Have optional navigation property: SpeciesInfo (one-to-one)
- Have navigation property: Rank (many-to-one)
- Mark orphaned parent_ids as nullable to prevent DbUpdateException
```

#### 3. Clean Architecture Layer Contracts

**Required Context**:

| Layer | Responsibility | Examples | Constraints |
|-------|-----------------|----------|-------------|
| **Persistence** | SQL Server database access via DbContext | DbSet queries, migrations | No business logic; only CRUD operations |
| **Infrastructure** | Repository implementations, Entity Framework mappings | TaxonRepository, IUnitOfWork | Dependency on Entity Framework; abstraction via Repository pattern |
| **Domain** | Entity definitions, business rules, specifications | Taxon entity, validation rules | No dependencies on infrastructure; defines what's valid |
| **Application** | Services coordinating domain and infrastructure | BirdSearchService, TaxonImporter | Depends on repositories; orchestrates workflows |
| **Controllers** | HTTP semantics, request/response mapping | BirdController, endpoints | Depends only on application services; no direct data access |

**Cross-Layer Rules**:

- Controllers never call repositories directly
- Application services inject repositories via constructor
- Repositories return domain entities, not DTOs
- DTOs exist only at API boundaries (Controllers ↔ Client)
- Domain entities never contain HTTP semantics

#### 4. Dependency Injection Configuration

**Required Context**:

- Service lifetime conventions (Singleton, Scoped, Transient)
- Repository registration pattern
- DbContext registration with connection string
- Service registration order and dependencies

**Example Pattern**:

```
DbContext: Scoped (per HTTP request)
Repositories: Scoped (depend on DbContext)
Services: Scoped (depend on repositories)
Controllers: Transient (depend on services)
```

#### 5. API Contract Specification

**Required Context**:

- Base URL: `/api/`
- Versioning strategy (none currently; `v1` planned for future)
- Standard response envelopes
- Error response format
- Pagination conventions

**HTTP Methods**:

```
GET    - retrieve without modification
POST   - create new resource; returns 201 with Location header
PUT    - replace entire resource; returns 200 with updated resource
DELETE - remove resource; returns 204 (No Content)
PATCH  - partial update (not currently supported; use PUT)
```

**Status Codes**:

```
200 - Success (with response body)
201 - Created (POST; includes Location header)
204 - No Content (DELETE success)
400 - Bad Request (validation error; includes error details)
401 - Unauthorized (missing/invalid authentication)
403 - Forbidden (authenticated but not authorized)
404 - Not Found (resource doesn't exist)
409 - Conflict (duplicate key, business rule violation)
500 - Internal Server Error (unhandled exception)
```

#### 6. DTO Specifications

**Required Context**:

- DTO naming convention: `{EntityName}Dto`
- Field inclusion rules (which domain properties are exposed)
- Data annotations for validation
- Required vs. optional fields
- Relationships and nested objects

**Example**:

```
TaxonDto must include:
- Id (int, required)
- ScientificName (string, required, max 255)
- CommonName (string, optional)
- RankId (int, required, valid rank reference)
- ParentId (int, optional, valid taxon reference if provided)
- Children (IEnumerable<TaxonDto>, optional, for hierarchical queries)

TaxonDto must NOT include:
- Internal flags or audit columns
- Database-specific details
- Sensitive information
```

#### 7. Naming Conventions

**Required Context**:

- Classes: PascalCase (`TaxonRepository`, `BirdSearchService`)
- Methods: PascalCase (`GetTaxonByIdAsync`, `SearchByScientificName`)
- Properties: PascalCase (`ScientificName`, `CommonName`)
- Private fields: camelCase with underscore (`_logger`, `_repository`)
- Constants: UPPER_CASE with underscores (`MAX_RESULTS = 100`)
- Database columns: snake_case (`scientific_name`, `common_name`)

#### 8. Async/Await Requirements

**Required Context**:

- All database operations must use async patterns (`async`/`await`)
- No `.Result`, `.Wait()`, or synchronous calls on async methods
- Task return types for async void methods (no fire-and-forget)
- Proper `ConfigureAwait(false)` for library code

#### 9. Exception Handling Strategy

**Required Context**:

- `TaxonException`: Custom exception for domain-level validation failures
- `SpeciesException`: Custom exception for species-specific logic violations
- `ArgumentNullException` for null parameters
- Don't catch and swallow exceptions (log or rethrow with context)
- HTTP layer translates exceptions to appropriate status codes

#### 10. Logging Standards

**Required Context**:

- Dependency: `ILogger<T>` injected via constructor
- Log levels: Debug (trace info), Information (major events), Warning (anomalies), Error (failures)
- Never log sensitive data (passwords, connection strings)
- Structured logging with named parameters for query analysis

### Out-of-Scope Context: What AI Models Should Avoid

#### 1. Database Migrations

AI should not generate migration files. Migrations require human review:

- Careful planning for backward compatibility
- Testing on production replicas
- Rollback strategy verification
- Data preservation for existing records

**Instead**: AI can generate entity configuration changes; humans create migrations.

#### 2. Architectural Changes

AI should not suggest architectural patterns conflicting with Clean Architecture. Established constraints:

- No database queries in controllers
- No business logic in persistence layer
- No direct dependency from controllers to infrastructure
- No mixing of concerns across layers

**Instead**: AI works within established patterns; humans evaluate architectural improvements.

#### 3. Security Configuration

AI should not generate authentication/authorization logic without explicit security context. Out-of-scope:

- Credential storage strategies
- OAuth/OpenID Connect flows
- Role-based access control policies
- Encrypted field handling

**Reason**: Security is domain-specific; generic patterns often introduce vulnerabilities.

#### 4. Database Performance Tuning

AI should not suggest index creation, query optimization, or execution plan changes without understanding production workloads.

**Instead**: AI implements queries following established patterns; humans profile and optimize.

#### 5. Infrastructure and DevOps

AI should not generate Dockerfile modifications, Docker Compose configurations, or deployment scripts without explicit infrastructure context.

**Instead**: AI generates application code; humans maintain infrastructure-as-code.

#### 6. Production Data Changes

AI should not generate scripts that manipulate production data directly.

**Instead**: AI generates queries and procedures; humans review and execute on production.

#### 7. Team Processes and Workflows

AI should not suggest changes to Git workflow, PR review process, or deployment pipelines without team consensus.

**Instead**: Humans establish processes; AI assists within agreed boundaries.

## Database Constraints

### Schema Constraints

| Constraint | Impact on Code Generation | Context Requirement |
|-----------|-------------------------|---------------------|
| `rank.name` unique | Cannot create duplicate ranks | Enforce uniqueness check before insertion |
| `taxon.parent_id` self-referential | Supports hierarchies; risk of cycles | Validate parent existence; prevent cyclic relationships |
| `taxon` unique(name, rank_id) | Same name allowed in different ranks | Include rank in unique constraint checks |
| `species_info.taxon_id` nullable | Not all taxa have species info | Use null-coalescing when accessing |
| Orphaned parent_id records | Legacy data quality issue | Handle gracefully; flag in logs; don't cascade delete |

### Query Pattern Constraints

| Query Type | Optimal Pattern | Anti-Pattern |
|-----------|-----------------|-------------|
| Single taxon + related rank | Single EF query with `.Include(t => t.Rank)` | Separate queries (N+1 problem) |
| Taxon hierarchy | Recursive CTE or `.Include(t => t.Children)` with depth limit | Unbounded recursion; multiple queries |
| Species search | Full-text search on `species_info` table | LIKE on every character (performance death) |
| Pagination | `.Skip().Take()` with counted total | Loading all results into memory |

### Connection and Performance Constraints

| Constraint | Impact | Mitigation |
|-----------|--------|-----------|
| Connection pooling limit (100 connections default) | Concurrent request limit | Use Scoped DbContext; don't hold connections open |
| SQL Server timeout (30s default) | Long-running queries fail | Paginate large result sets; add indexes for sort columns |
| Memory limitations | Large hierarchies cause OutOfMemory | Use lazy loading or projection to `Dto` with limited fields |

## API Contract Constraints

### Request Constraints

**Pagination Parameters**:

```
GET /api/taxa?page=1&pageSize=20&rankId=1&parentId=5

Constraints:
- page: int, minimum 1, default 1
- pageSize: int, minimum 1, maximum 100, default 20
- rankId: int, optional, must reference valid rank
- parentId: int, optional, must reference valid taxon if provided
```

**Search Parameters**:

```
GET /api/taxa/search?q=felis&type=scientific

Constraints:
- q: string, required, minimum 2 characters, maximum 255
- type: string, enum(scientific|common), default scientific
- Exact match not supported; pattern matching case-insensitive
```

### Response Constraints

**Single Entity Response**:

```json
{
  "id": 123,
  "scientificName": "Felis catus",
  "commonName": "Domestic Cat",
  "rankId": 7,
  "parentId": 456,
  "createdAt": "2025-01-15T10:30:00Z",
  "modifiedAt": "2025-03-10T14:20:00Z"
}
```

**List Response with Pagination**:

```json
{
  "data": [
    { "id": 1, "scientificName": "...", ... },
    { "id": 2, "scientificName": "...", ... }
  ],
  "pagination": {
    "currentPage": 1,
    "pageSize": 20,
    "totalCount": 456,
    "totalPages": 23
  }
}
```

**Error Response**:

```json
{
  "error": {
    "code": "VALIDATION_ERROR",
    "message": "Validation failed",
    "details": [
      {
        "field": "scientificName",
        "message": "Scientific name is required"
      }
    ]
  }
}
```

## Architectural Constraints

### Layer Isolation Rules

**Controllers** may:
- Accept HTTP requests
- Validate input parameters
- Call application services
- Return HTTP responses
- Log at Information level (major events)

**Controllers** may NOT:
- Query repositories directly
- Execute business logic
- Access DbContext
- Manipulate domain entities

**Application Services** may:
- Depend on repositories (injected)
- Orchestrate complex workflows
- Enforce business rules
- Translate domain entities to DTOs
- Call other services

**Application Services** may NOT:
- Accept HTTP requests
- Return HTTP responses
- Query DbContext directly
- Depend on Controllers

**Repositories** may:
- Query DbSet through EF Core
- Apply filters and projections
- Return domain entities
- Handle Entity Framework exceptions

**Repositories** may NOT:
- Contain business logic
- Return DTOs
- Call other repositories
- Execute raw SQL (except migrations)

### Dependency Direction

```
Controllers → Application Services → Repositories → DbContext
                                  → Domain Entities
```

Reverse dependencies (e.g., DbContext knowing about Controllers) are forbidden.

### Interface Segregation

```
public interface ITaxonRepository
{
    Task<Taxon> GetByIdAsync(int id);
    Task<IEnumerable<Taxon>> GetByRankAsync(int rankId);
    Task<Taxon> AddAsync(Taxon taxon);
}

public interface ISpeciesInfoRepository
{
    Task<SpeciesInfo> GetByTaxonIdAsync(int taxonId);
    Task<SpeciesInfo> AddAsync(SpeciesInfo info);
}
```

Each repository handles a single entity; don't mix unrelated entities.

## Agent Instruction Constraints

### Constraints for GitHub Copilot

Copilot operates in IDE context with limited project visibility:

- **File scope**: Limited to open files and recent edits
- **Context window**: Approximately 3000 tokens of surrounding code
- **Usage**: Incremental code completion and refactoring

**Constraints**:

- Don't assume Copilot knows about classes in other files
- Provide explicit type hints when using generic types
- Include class/interface signatures in prompts
- Verify generated code against architecture before accepting

### Constraints for OpenAI Codex

Codex operates in chat/notebook context with explicit context provision:

- **Context window**: Full prompt capacity (can include entire classes)
- **Usage**: Architecture discussion, documentation, design review
- **Capability**: Stronger reasoning about cross-cutting concerns

**Constraints**:

- Provide complete architectural context in prompts
- Clarify layer boundaries before asking for implementations
- Validate generated documentation against actual codebase
- Don't assume Codex knows about private architectural decisions

### Constraints for Claude

Claude operates with large context windows and strong reasoning:

- **Context window**: 200K tokens; can include entire codebases
- **Usage**: Complex architectural decisions, comprehensive documentation, refactoring planning
- **Capability**: Strong cross-file and cross-layer reasoning

**Constraints**:

- Verify domain knowledge matches current codebase state
- Claude may suggest improvements; evaluate against established patterns
- Ask explicitly to flag any deviations from established constraints
- Use Claude for validation of work from other models

### Constraints for NotebookLM

NotebookLM specializes in document understanding and synthesis:

- **Context**: Documents, specifications, design docs
- **Usage**: Knowledge synthesis, cross-reference analysis, documentation generation
- **Capability**: Connecting concepts across multiple sources

**Constraints**:

- Input documents must be current and accurate
- NotebookLM is suitable for documentation only, not code generation
- Validate synthesized knowledge against actual implementations
- Use for training materials and onboarding documents

## Context Persistence Strategy

### Context Storage Structure

```
Docs/
├── Architecture/
│   ├── clean-architecture.md        # Layer definitions
│   ├── repository-pattern.md         # Repository contracts
│   └── dependency-injection.md       # DI configuration
├── Database/
│   ├── schema.md                     # Table definitions and relationships
│   ├── constraints.md                # Legacy constraints and quirks
│   └── query-patterns.md             # Optimal EF Core query patterns
├── API/
│   ├── endpoints.md                  # REST endpoint specifications
│   ├── dto-specifications.md         # DTO field definitions
│   ├── status-codes.md               # HTTP status code semantics
│   └── pagination.md                 # Pagination standards
├── Domain/
│   ├── entities.md                   # Domain entity definitions
│   ├── business-rules.md             # Validation and business logic rules
│   └── exceptions.md                 # Custom exception usage
├── AI/
│   ├── context-manifest.md           # This file
│   ├── prompt-library.md             # Approved prompts
│   ├── context-update-log.md         # Version history
│   └── agent-guidelines.md           # Per-model constraints
└── Code/
    └── conventions.md                # Naming, formatting standards
```

### Context Manifest

Maintain `Docs/AI/context-manifest.md` listing all context documents:

| Document | Purpose | Last Updated | Version | Status |
|----------|---------|--------------|---------|--------|
| Architecture/clean-architecture.md | Layer definitions | 2025-04-20 | 1.3 | Current |
| Database/schema.md | Table and relationship specs | 2025-04-15 | 2.1 | Current |
| API/endpoints.md | REST endpoint contracts | 2025-04-10 | 1.8 | Current |
| Domain/business-rules.md | Validation rules | 2025-03-28 | 1.2 | Needs Update |

### Context in Code Comments

Embed context directly in code for discoverable knowledge:

```csharp
/// <summary>
/// Repository for taxonomic unit (taxon) operations.
/// 
/// ARCHITECTURAL CONTEXT:
/// This repository implements the Repository pattern, abstracting
/// Entity Framework Core queries. Consumers should depend on the
/// ITaxonRepository interface, never DbContext directly.
/// 
/// DATABASE CONTEXT:
/// The taxon table includes a self-referential parent_id supporting
/// hierarchical relationships (Kingdom → Phylum → ... → Species).
/// Note: Orphaned parent_ids exist in legacy data; handle gracefully.
/// 
/// CARDINALITY:
/// - One rank has many taxa
/// - One taxon has many child taxa (self-referential)
/// - One taxon has zero or one SpeciesInfo (1:0..1)
/// </summary>
public interface ITaxonRepository
{
    // ...
}
```

### Context in README

Include key context in the project README:

```markdown
## Architecture Overview

BirdTaxonomy API follows Clean Architecture with distinct layers:

- **Controllers**: HTTP request/response handling
- **Application**: Business logic and service orchestration
- **Domain**: Entity definitions and business rules
- **Infrastructure**: Repository implementations, Entity Framework
- **Persistence**: Database access via DbContext

See [Architecture Documentation](Docs/Architecture/clean-architecture.md) for detailed patterns.

## Database

This API wraps a legacy SQL Server taxonomy database with three primary tables:

- `rank`: Taxonomic rank definitions (Kingdom, Phylum, etc.)
- `taxon`: Biological taxa with hierarchical relationships
- `species_info`: Extended information for species-level taxa

See [Database Schema](Docs/Database/schema.md) for detailed constraints and relationships.

## Key Constraints

- All database operations must use async/await
- Layer boundaries are strictly enforced
- DTOs are used only at API boundaries
- Repositories abstract all database access
- Custom exceptions for domain-level failures

See [API Contracts](Docs/API/endpoints.md) and [Domain Rules](Docs/Domain/business-rules.md) for complete specifications.
```

## Context Update Strategy

### When to Update Context

**Trigger Events**:

1. **Schema Changes**: New tables, columns, or relationship modifications
2. **API Evolution**: New endpoints, response format changes, status code additions
3. **Architectural Changes**: New patterns, changed layer boundaries
4. **Constraint Changes**: Performance requirements, new business rules
5. **Standard Evolution**: Updated naming conventions, new frameworks adopted

### Update Process

1. **Assessment**: Determine which documents are affected
2. **Drafting**: Update documents with new information
3. **Validation**: Code walkthrough to ensure accuracy
4. **Approval**: Architecture review before acceptance
5. **Distribution**: Notify team of changes; update context in README
6. **Version Bump**: Increment manifest version numbers

### Version Control for Context

```bash
git log --oneline Docs/Database/schema.md
a1b2c3d (2025-04-20) Update taxon cardinality rules: add orphan handling
f4e5d6c (2025-03-15) Add legacy constraint notes for rank table
7g8h9i0 (2025-02-01) Initial schema documentation
```

## Risks of Context Drift

### Risk 1: Outdated Schema Understanding

**Symptom**: AI generates code based on incorrect column names or table relationships.

**Cause**: Database was modified; documentation not updated.

**Prevention**:
- Update schema documentation immediately after migrations
- Include schema version number in schema.md
- Add automated schema comparison in CI/CD (optional)

**Mitigation**:
- Code review catches mismatched entity mappings
- Entity Framework throws exceptions on invalid column names

### Risk 2: Architectural Decay

**Symptom**: New code violates layer boundaries; repositories contain business logic.

**Cause**: Architectural documentation outdated or not enforced.

**Prevention**:
- Document architectural decisions with rationale
- Include architecture diagrams
- Add code review checklist for architectural violations

**Mitigation**:
- Dependency analysis tools detect cross-layer violations
- Code review enforces layer separation
- New team members onboard with architecture documentation

### Risk 3: API Contract Divergence

**Symptom**: Endpoints return unexpected status codes or response formats.

**Cause**: API evolves without updating contract documentation.

**Prevention**:
- Document endpoint specs before implementation
- Update documentation alongside code
- Include Swagger/OpenAPI specification

**Mitigation**:
- API integration tests validate status codes and schema
- Swagger UI reflects actual implementation
- Client SDKs generated from OpenAPI specification

### Risk 4: Naming Convention Inconsistency

**Symptom**: Classes named inconsistently (BirdService vs BirdsService; GetBird vs FetchBird).

**Cause**: Naming conventions not explicit or not enforced.

**Prevention**:
- Document naming rules explicitly
- Include examples from existing code
- Use IDE code analysis to enforce conventions

**Mitigation**:
- Code review enforces consistency
- Rename refactorings align inconsistent names
- EditorConfig file enforces style conventions

### Risk 5: Async/Await Pattern Drift

**Symptom**: Code uses `.Result` or `.Wait()` blocking calls.

**Cause**: Async requirement not emphasized in context.

**Prevention**:
- Document async requirement with rationale
- Include examples of correct async patterns
- Highlight common mistakes

**Mitigation**:
- Code analysis warns on `.Result` and `.Wait()`
- Unit tests enforce async method signatures
- Code review catches blocking calls

### Risk 6: Exception Handling Inconsistency

**Symptom**: Some methods throw custom exceptions; others return error codes.

**Cause**: Exception strategy not clearly documented or enforced.

**Prevention**:
- Document exception hierarchy
- Include decision rationale
- Provide examples of correct usage

**Mitigation**:
- Code review verifies exception usage
- Unit tests validate exception conditions
- Update project standards when strategy evolves

### Risk 7: Dependency Injection Misconfiguration

**Symptom**: Services can't be injected; mismatched lifetimes cause issues.

**Cause**: DI configuration rules not documented or understood.

**Prevention**:
- Document DI registration patterns
- Include service lifetime rules
- Explain Scoped vs Transient lifetime effects

**Mitigation**:
- Startup exceptions highlight missing registrations
- Runtime errors indicate lifetime mismatches
- Code review verifies DI configuration

## Mitigation Strategies

### Strategy 1: Regular Context Audits

**Frequency**: Quarterly

**Process**:
1. Sample random classes from codebase
2. Compare implementation against documented context
3. Flag any discrepancies
4. Update documentation or code accordingly
5. Document audit results in Docs/AI/context-audit-log.md

**Success Criteria**: 100% alignment between documentation and codebase

### Strategy 2: Automated Context Validation

**Tools**:
- FxCop / Roslyn analyzers: Enforce naming conventions, detect code issues
- Entity Framework logging: Validate database access patterns
- Swagger/OpenAPI: Ensure endpoint documentation matches implementation
- Static analysis: Detect architectural violations

**Example**: 

```csharp
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class AsyncCallAnalyzer : DiagnosticAnalyzer
{
    // Flag usage of .Result, .Wait() on async operations
    // Enforce async/await patterns
}
```

### Strategy 3: Documentation-Driven Development

**Process**:
1. Update relevant context documents first
2. Implement code matching documented context
3. Code review verifies alignment
4. Merge only when both code and documentation are consistent

**Benefit**: Prevents documentation-code divergence at source.

### Strategy 4: Context Injection in Prompts

**Practice**: Always include relevant context excerpts in AI prompts.

**Example**:

```
[From Docs/Database/schema.md]
The taxon table has a self-referential parent_id supporting hierarchies.
Orphaned parent_ids exist in legacy data and must be handled gracefully.
Note: The species_info table may have records for non-species taxa due to 
historical data quality issues.

[From Docs/Architecture/clean-architecture.md]
Repositories abstract all database access. Controllers must not query DbContext.
All database operations must use async/await.

Task: Create a method to retrieve a taxon by ID along with its species info.
```

### Strategy 5: Context Change Notifications

**Process**:
- Tag all context updates with `[CONTEXT]` in commit messages
- Include changelog entry in Docs/AI/context-update-log.md
- Notify team on architectural Slack channel
- Add to sprint retro for awareness

**Example**:

```
[CONTEXT] Update schema documentation: add orphan handling notes

- Added section on orphaned parent_id records in legacy data
- Clarified correct handling in repository implementations
- Version: schema.md v2.2

Breaking change: No
Migration required: No
Code review focus: Verify null-safe access to parent taxon
```

### Strategy 6: Onboarding Context Checklist

**New Developer Onboarding**:

- [ ] Read Docs/Architecture/clean-architecture.md
- [ ] Read Docs/Database/schema.md and understand table relationships
- [ ] Review API endpoints in Docs/API/endpoints.md
- [ ] Study example implementation (e.g., TaxonRepository, BirdService)
- [ ] Complete "Adding a Simple Endpoint" exercise
- [ ] Understand exception hierarchy in Docs/Domain/exceptions.md
- [ ] Review naming conventions in Docs/Code/conventions.md

**Success Criteria**: New developer can implement small feature without architecture guidance.

### Strategy 7: AI-Generated Context Validation

**Process**:

When using AI to generate documentation updates:

1. AI generates documentation updates based on code analysis
2. Human architect reviews for accuracy and completeness
3. Validate against codebase with code samples
4. Domain expert (taxonomist) reviews scientific accuracy
5. Merge only after validation passes

**Example**:

```
AI-Generated: Updated species_info relationships

Review checklist:
[ ] Cardinality is correct (1:0..1 relationship)
[ ] Null-safety handling is documented
[ ] Examples match actual code
[ ] Scientific terms used correctly
[ ] No contradictions with other docs
```

## Conclusion

Context engineering is the foundation enabling reliable AI-assisted development. By establishing clear context boundaries, maintaining accurate documentation, and systematically addressing drift, the BirdTaxonomy API team ensures that AI models generate code consistent with architecture, database constraints, and API contracts.

Context is never static. As the system evolves, context must evolve alongside it. Regular audits, automated validation, and documentation-driven development practices keep context synchronized with reality.

The investment in context engineering pays dividends: AI assistance becomes predictable and reliable, reducing rework and enabling developers to focus on high-value problem-solving rather than fighting the tool.