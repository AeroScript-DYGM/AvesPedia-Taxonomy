# Agent Workflow for BirdTaxonomy API

## Overview

The BirdTaxonomy API development process leverages multiple AI agents, each with distinct capabilities and operational contexts. This document specifies how these agents collaborate, when each is invoked, what artifacts they produce, and how their outputs are validated and integrated.

## Multi-Agent Architecture

### Agent Roster

| Agent | Model | Context Type | Primary Use | Invocation |
|-------|-------|-------------|------------|-----------|
| **Copilot** | GitHub Copilot | IDE-embedded | Incremental code completion, in-file refactoring | Automatic (developer-initiated) |
| **Codex** | OpenAI Codex | Chat/Notebook | Architecture discussion, documentation, design review | Manual (developer request) |
| **Claude** | Claude 3.5 Sonnet | Conversational chat | Complex reasoning, comprehensive refactoring, validation | Manual (developer request) |
| **NotebookLM** | Google NotebookLM | Document synthesis | Knowledge synthesis, training materials, cross-reference analysis | Manual (PM/documentation request) |

### Agent Roles in System Context

```
┌─────────────────────────────────────────────────────────────┐
│                    BirdTaxonomy Development                  │
├─────────────────────────────────────────────────────────────┤
│                                                               │
│  Copilot         Codex           Claude        NotebookLM    │
│  ├─ IDE context  ├─ Chat context ├─ Chat      ├─ Document   │
│  ├─ File-level   ├─ Project      ├─ Codebase  │  Synthesis  │
│  └─ Real-time    │   overview    └─ Analysis  └─ Training   │
│                  └─ Discussion                              │
│                                                               │
│             ↓ Coordinated via Git + Code Review              │
│                                                               │
│  Validated Artifacts (Code + Documentation)                 │
│  ├─ Commits to main branch                                  │
│  ├─ Documentation in Docs/                                  │
│  └─ Deploy to MonsterASP.NET                                │
│                                                               │
└─────────────────────────────────────────────────────────────┘
```

## Agent Capabilities and Constraints

### GitHub Copilot

**Capabilities**:

- Real-time code completion within IDE
- Intelligent method stub generation
- Refactoring suggestions for selected code
- Test generation from production code
- Comment-to-code translation
- Error fix suggestions from compiler messages

**Context Window**:

- Approximately 3000 tokens
- Limited to current file + recently opened files
- IDE symbol resolution (classes visible in project)
- Git history awareness (recent commits)

**Constraints**:

- Cannot see entire codebase context
- Limited to single-file refactoring
- May generate code conflicting with distant architectural decisions
- No access to architecture documentation directly

**Best For**:

- Completing method bodies matching surrounding code style
- Generating boilerplate (getters, setters, constructors)
- Test method stubs
- Simple refactorings within a class
- Fixing compiler errors

**Worst For**:

- Cross-layer architectural changes
- Database schema modifications
- API contract changes
- Complex business logic design
- Documentation of system-wide patterns

**Validation Requirements**:

- Code review for architecture compliance before commit
- Compilation without warnings
- Test execution passing
- No references to undefined classes/methods

### OpenAI Codex

**Capabilities**:

- Comprehensive code generation from natural language specification
- Multi-class system design and implementation
- API endpoint generation with complete Request/Response handling
- Test case generation (unit, integration)
- Documentation generation (API docs, README sections)
- Query optimization suggestions
- Exception handling pattern generation

**Context Window**:

- 4000-8000 tokens depending on usage
- Can include code snippets, API specs, architecture guidelines
- Capable of understanding cross-file relationships
- Schema and constraint information inclusion

**Constraints**:

- Chat-based; no real-time IDE integration
- Cannot directly access file system (requires copy-paste)
- May hallucinate details not explicitly provided
- Requires explicit project context in prompt
- No automatic language-specific awareness (must specify C# ASP.NET Core)

**Best For**:

- Designing new features across multiple classes
- Generating complete service implementations
- Creating comprehensive test suites
- Writing technical documentation
- API specification and contract definition
- Refactoring guidance and strategy

**Worst For**:

- Real-time code completion
- Debugging specific runtime errors
- File-by-file incremental work
- Complex reasoning about existing code without context
- Sensitive operations (authentication, encryption)

**Validation Requirements**:

- Architecture review for layer separation
- Code review for clean code principles
- Test coverage verification
- Documentation accuracy validation
- Integration testing before merge

### Claude (3.5 Sonnet)

**Capabilities**:

- Deep code analysis and comprehension
- Multi-file refactoring planning and execution
- Architectural decision documentation
- Complex reasoning about legacy system integration
- Comprehensive documentation synthesis
- Code quality analysis and improvement recommendations
- Conflict resolution between agents
- Validation of other agents' outputs

**Context Window**:

- 200,000 tokens (largest of all agents)
- Can include entire repository file listings
- Complete architecture documentation
- Database schemas and constraints
- Historical commit messages and design rationale

**Constraints**:

- Chat-based interface; not real-time IDE integration
- May be slower response time due to large context processing
- Requires explicit direction; less autonomous
- Cannot directly modify files (human-mediated)

**Best For**:

- Comprehensive system analysis and documentation
- Multi-file refactoring strategy
- Validation of outputs from other agents
- Complex architectural decisions
- Training material and knowledge synthesis
- Identifying technical debt and improvement opportunities
- Cross-system impact analysis

**Worst For**:

- Real-time code completion
- Rapid iteration on small changes
- Interactive debugging
- Performance-critical path optimization (without profiling data)

**Validation Requirements**:

- Independent review of recommendations
- Feasibility assessment by domain experts
- Integration testing of large refactorings
- Stakeholder approval for architectural changes

### Google NotebookLM

**Capabilities**:

- Document synthesis across multiple sources
- Knowledge base creation from API docs and architecture specs
- Cross-reference analysis between documents
- Training material generation
- Glossary and terminology extraction
- FAQ generation from documentation
- Interactive Q&A over document corpus

**Context**:

- Document-based (PDFs, Markdown, text)
- No code understanding (understands documentation about code)
- Synthesis across multiple documents
- Citation and source tracking

**Constraints**:

- Requires input documents to be accurate and current
- Not suitable for code generation
- Document quality affects synthesis quality
- No real-time integration

**Best For**:

- Creating onboarding documentation
- Generating FAQ from architecture documentation
- Synthesizing knowledge from dispersed documents
- Creating training materials
- Glossary of domain-specific terminology
- Knowledge base queries over documentation

**Worst For**:

- Code generation
- Real-time development assistance
- Interactive problem-solving
- Debugging
- Architecture design (use Claude instead)

**Validation Requirements**:

- Verification against source documents
- Accuracy check by domain experts
- Testing training materials with new developers
- Update synchronization with source docs

## When Each Agent is Invoked

### Development Workflow Timeline

```
Feature Request
    ↓
[Claude] - Architecture & Design Planning (async, human-initiated)
    ↓
Approved Design Specification
    ↓
[Codex] - Endpoint and Service Implementation (async, human-initiated)
    ↓
Generated Code + Tests
    ↓
[Copilot] - Local refactoring & completion (real-time, automatic)
    ↓
Code Review (human)
    ↓
[Claude] - Validation review (async, human-initiated)
    ↓
Approval
    ↓
Merge to main
    ↓
[NotebookLM] - Documentation synthesis (async, PM-initiated)
    ↓
Release Documentation
```

### Decision Tree: Which Agent to Use

```
Task: Add new feature

├─ Design phase? (new endpoints, architecture questions)
│  ├─ Complex, system-wide impact?
│  │  └─ Use Claude for comprehensive architecture planning
│  └─ Single endpoint, straightforward service?
│     └─ Use Codex for implementation blueprint
│
├─ Implementation phase?
│  ├─ Writing code right now?
│  │  └─ Use Copilot for real-time completion in IDE
│  ├─ Generating multiple classes/tests?
│  │  └─ Use Codex (chat-based) to design, then Copilot to refine
│  └─ Refactoring multiple files?
│     └─ Use Claude to plan, Copilot to execute
│
├─ Review phase?
│  ├─ Architecture compliance?
│  │  └─ Use Claude for comprehensive review
│  └─ Code quality?
│     └─ Use Copilot suggestions + human review
│
└─ Documentation phase?
   ├─ Comprehensive onboarding docs?
   │  └─ Use NotebookLM to synthesize from architecture docs
   ├─ API documentation?
   │  └─ Use Codex for spec, Claude for review
   └─ Training materials?
      └─ Use NotebookLM with approved documentation
```

## Artifact Flows

### Input Artifacts

#### For Copilot

| Artifact | Source | Format | Usage |
|----------|--------|--------|-------|
| Current file | IDE | C# source code | Real-time completion context |
| Open files | IDE | C# source code | Symbol resolution, import suggestions |
| Recent changes | Git | Diff format | Understanding recent patterns |
| Error messages | Compiler | Text output | Context for error-fix suggestions |
| Test failures | Test runner | Text output | Test-fix suggestions |

#### For Codex

| Artifact | Source | Format | Usage |
|----------|--------|--------|-------|
| Feature specification | Requirements doc | Markdown text | Design basis |
| Existing code patterns | Repository | Code snippets | Style/pattern matching |
| API schema | OpenAPI spec | JSON/YAML | Contract definition |
| Database schema | Schema.md | Markdown/SQL | Data access patterns |
| Architecture guidelines | Docs/Architecture | Markdown | Layer compliance |
| Exception patterns | Domain/exceptions.md | Markdown | Error handling strategy |

#### For Claude

| Artifact | Source | Format | Usage |
|----------|--------|--------|-------|
| Repository codebase | Git | File tree + samples | Comprehensive analysis |
| Architecture documentation | Docs/Architecture | Markdown | Design decisions |
| Database schema + constraints | Docs/Database | Markdown + SQL | Legacy system understanding |
| API specification | Docs/API | Markdown/OpenAPI | Contract understanding |
| Git history | Git | Commit logs + diffs | Decision rationale |
| Test coverage report | Code analysis | Text report | Quality assessment |
| Prompt library | Docs/AI | Markdown | Learning approved patterns |

#### For NotebookLM

| Artifact | Source | Format | Usage |
|----------|--------|--------|-------|
| Architecture documentation | Docs/Architecture | Markdown/PDF | Source knowledge base |
| API endpoint specification | Docs/API | Markdown | API training docs |
| Database schema docs | Docs/Database | Markdown | Schema understanding |
| Business rules | Docs/Domain | Markdown | Domain training |
| Glossary | Docs/Domain | Markdown | Terminology reference |
| Example code | Repository | Markdown snippets | Pattern examples |

### Output Artifacts

#### Copilot Outputs

| Artifact | Destination | Validation | Notes |
|----------|-------------|-----------|-------|
| Code completions | Current file | Compilation, review | Accepted/rejected by developer |
| Method stubs | Current file | Compilation | Developer completes logic |
| Test templates | Test file | Compilation | Developer adds test cases |
| Refactoring suggestions | Review pane | Developer choice | Interactive acceptance |
| Error fixes | Current file | Compilation | May require additional context |

#### Codex Outputs

| Artifact | Destination | Validation | Notes |
|----------|-------------|-----------|-------|
| Service implementations | Codex response | Code review, testing | Paste into IDE/repository |
| Controller endpoints | Codex response | API testing, review | May require manual adjustment |
| Test classes | Codex response | Test execution | Run `dotnet test` to validate |
| API documentation | Codex response | Comparison with endpoints | May require accuracy fixes |
| Database queries | Codex response | Plan review, testing | No raw SQL in app code |

#### Claude Outputs

| Artifact | Destination | Validation | Notes |
|----------|-------------|-----------|-------|
| Architecture recommendations | Email/issue | Stakeholder review | Document as ADR (Architecture Decision Record) |
| Refactoring plans | GitHub issue | Team discussion | May span multiple sprints |
| Code review feedback | Pull request comments | Author response | Informs code changes |
| Documentation updates | Docs/ directory | Accuracy verification | May require domain expert review |
| Validation reports | Development notes | Stakeholder awareness | Informs next steps |

#### NotebookLM Outputs

| Artifact | Destination | Validation | Notes |
|----------|-------------|-----------|-------|
| Onboarding guide | Docs/Onboarding | New dev testing | Try with actual new developer |
| FAQ document | Docs/FAQ | Accuracy check | Verify answers against codebase |
| Training materials | Docs/Training | Instructor review | Tested with trainees |
| Glossary | Docs/Glossary | Domain expert approval | Scientific accuracy critical |
| Knowledge base queries | Email/Slack | Stakeholder consumption | Links to source documents |

## Human Validation Checkpoints

### Checkpoint 1: Design Review

**Trigger**: After Claude proposes architecture or Codex proposes design

**Participants**: 
- Lead architect
- Senior developer
- Domain expert (optional, for taxonomy-specific decisions)

**Validation Checklist**:

- [ ] Design respects Clean Architecture layers
- [ ] Dependencies follow inversion principle
- [ ] Database queries match schema constraints
- [ ] API contract is RESTful and consistent
- [ ] Scalability considered for projected load
- [ ] Backward compatibility assessed
- [ ] Testing strategy identified

**Decision**: Approved / Request Changes / Rejected

**Duration**: 1-2 hours (sync meeting or async review)

### Checkpoint 2: Code Review

**Trigger**: After implementation (Copilot-completed code or Codex-generated code)

**Participants**:
- Code reviewer (different from implementer)
- Original developer (available for questions)
- Optional: architecture reviewer

**Validation Checklist**:

- [ ] Compiles without errors/warnings
- [ ] Tests pass locally
- [ ] Code follows naming conventions
- [ ] Layer boundaries respected
- [ ] No direct database access in controllers
- [ ] Exception handling complete
- [ ] DTOs used correctly at boundaries
- [ ] Async/await used correctly
- [ ] No N+1 queries (EF Core logging verified)
- [ ] Documentation complete (XML comments)
- [ ] No hardcoded values or connection strings

**Tools**:
- SonarQube for code quality metrics
- EF Core query logging for N+1 detection
- IDE inspections for naming violations

**Decision**: Approved / Request Changes / Rejected

**Duration**: 30 minutes to 2 hours depending on change size

### Checkpoint 3: Integration Testing

**Trigger**: After code review approval, before merge

**Participants**:
- QA engineer (automated + manual)
- Feature owner (functional verification)

**Validation**:

- [ ] Endpoint responds correctly for valid requests
- [ ] Error responses match specification (status codes, format)
- [ ] Database state changes as expected
- [ ] No regression in existing functionality
- [ ] Performance acceptable (query time, response size)
- [ ] Pagination works correctly
- [ ] Swagger documentation matches implementation

**Tools**:
- Postman collection for endpoint testing
- Integration test suite in `xUnit`
- Database state verification scripts

**Duration**: 1-4 hours depending on feature complexity

### Checkpoint 4: Claude Validation Review

**Trigger**: For complex changes or refactorings; optional for simple features

**Participants**:
- Claude (provided with code + documentation)
- Architecture reviewer (to evaluate Claude's findings)

**Validation Questions**:

- Does implementation match architecture specification?
- Are there performance concerns?
- Are there security issues?
- Is documentation accurate?
- Are there improvements not immediately obvious?
- Does this set precedent that should be documented?

**Output**: Validation report with findings and recommendations

**Duration**: 2-4 hours (async Claude interaction + human review)

### Checkpoint 5: Documentation Update

**Trigger**: After merge; before release

**Participants**:
- Technical writer
- Domain expert
- NotebookLM (for synthesis)

**Validation**:

- [ ] API documentation updated
- [ ] Architecture documentation reflects changes
- [ ] Database schema docs updated if needed
- [ ] Examples in documentation work
- [ ] Release notes drafted
- [ ] Training materials updated

**Duration**: 2-8 hours depending on documentation scope

## Conflict Resolution Between Agents

### Scenario 1: Copilot vs. Architecture Standards

**Situation**: Copilot suggests code that violates Clean Architecture (e.g., controller calling DbContext directly).

**Resolution Process**:

1. **Detection**: Code reviewer flags violation during review
2. **Analysis**: Why did Copilot violate standard?
   - Insufficient context in file
   - Copilot learned from older code in repo
   - Surrounding code is inconsistent
3. **Correction**: Reject suggestion; provide correct pattern
4. **Prevention**: 
   - Ensure old code is refactored to standard patterns
   - Add inline architecture comments near Copilot-risky areas
   - Include architecture documentation in code files

**Example**:

```csharp
// BAD (Copilot may suggest this)
public class BirdController : ControllerBase
{
    private readonly BirdTaxonomyDbContext _context;
    
    [HttpGet("{id}")]
    public async Task<ActionResult<Bird>> GetBird(int id)
    {
        var bird = await _context.Taxa.FirstOrDefaultAsync(t => t.Id == id);
        // ...
    }
}

// GOOD (Correct pattern)
public class BirdController : ControllerBase
{
    private readonly IBirdService _service;
    
    public BirdController(IBirdService service) => _service = service;
    
    [HttpGet("{id}")]
    public async Task<ActionResult<BirdDto>> GetBird(int id)
    {
        var bird = await _service.GetBirdByIdAsync(id);
        // ...
    }
}
```

### Scenario 2: Codex vs. Database Constraints

**Situation**: Codex generates query logic that ignores database constraints (e.g., orphaned parent_id).

**Resolution Process**:

1. **Detection**: Code review or integration testing reveals issue
2. **Analysis**: Does Codex prompt include constraint information?
3. **Correction**: Reject code; add constraint details to new prompt
4. **Learning**: Update Docs/Database/schema.md with detailed constraint examples
5. **Resubmission**: Re-run Codex with enhanced context

**Example**:

```
ORIGINAL CODEX PROMPT (insufficient):
"Generate a method to get parent taxon"

CORRECTED PROMPT:
"Generate a method to get parent taxon for a given taxon.
Note: The database has legacy data with orphaned parent_id references.
If the parent taxon doesn't exist in the database, return null (don't throw).
Handle this gracefully with null-coalescing."
```

### Scenario 3: Claude vs. Copilot Outputs

**Situation**: Claude review of Copilot code identifies issues not caught in code review.

**Resolution Process**:

1. **Detection**: Claude notes architectural concern in validation review
2. **Impact Assessment**: Is this critical? Can it be fixed incrementally?
3. **Decision**:
   - **Critical**: Revert change; plan refactoring with architect
   - **Important**: Create tech debt issue; schedule refactoring
   - **Nice-to-have**: Document for future refactoring
4. **Prevention**: Use Claude for pre-review validation on complex changes

**Example Claude Finding**:

```
Analysis: Method GetTaxaWithSpecies() issues:

1. PERFORMANCE: N+1 query detected. For each taxon in the list, 
   the code queries species_info separately. Should use 
   .Include(t => t.SpeciesInfo).

2. ARCHITECTURE: Service layer is querying DbContext directly 
   instead of using repository. Violates Repository pattern.

3. DOCUMENTATION: Missing null handling for species_info 
   (relationship is 1:0..1).

Recommendation: Mark for refactoring in next sprint.
```

### Scenario 4: NotebookLM vs. Current Implementation

**Situation**: NotebookLM generates training docs that conflict with current code.

**Resolution Process**:

1. **Detection**: New developer uses training materials; code doesn't match
2. **Analysis**: Did code change after documentation? Is docs outdated?
3. **Correction**: 
   - If code changed: Update NotebookLM source documents + regenerate
   - If docs wrong: Correct source documents before NotebookLM synthesis
4. **Prevention**: 
   - Treat documentation as source of truth
   - Run NotebookLM synthesis only after all changes merged
   - Version documentation alongside code

### Scenario 5: Multiple Agents Generated Conflicting Code

**Situation**: Copilot added method; Codex generated service; methods conflict.

**Resolution Process**:

1. **Detection**: Compilation error or naming conflict
2. **Analysis**: Which agent's output is better?
3. **Decision**:
   - Merge compatible parts
   - Reject weaker version
   - Human reconciles manually
4. **Prevention**: 
   - Serialize agent invocations (don't run Copilot + Codex simultaneously on same file)
   - Code review must consider both outputs
   - One developer owns file during edit session

## Version Control Workflow

### Branch Strategy

```
main (production)
  ↑
integration (pre-release; deployment candidate)
  ↑
develop (integration branch; in-development features)
  ↑
feature/* (individual feature branches)
```

### Feature Branch Workflow

```
1. Create feature branch:
   git checkout -b feature/add-species-search

2. Invoke agents (Claude for design, Codex for implementation):
   - Claude generates architecture specification
   - Codex generates service + controller + tests
   - Copilot refines in IDE

3. Commit AI-assisted code:
   git add .
   git commit -m "feat: implement species search endpoint
   
   - Added SpeciesSearchService with pattern matching
   - Added /api/species/search endpoint with pagination
   - Generated tests for happy path and error cases
   - AI-assisted: Codex + Copilot
   - Human review: [reviewer name]"

4. Code review (human):
   - Request changes if needed
   - Copilot suggestions reviewed
   - Architecture validated

5. Validation (optional Claude review):
   - Claude reviews for cross-layer compliance
   - Recommends refactoring if needed

6. Merge to develop:
   git checkout develop
   git merge feature/add-species-search
   git push origin develop

7. Documentation (NotebookLM):
   - Update API docs
   - Update training materials
   - Commit to Docs/

8. Merge to integration (pre-release):
   git checkout integration
   git merge develop
   git tag v0.2.0-rc1

9. Release to production:
   git checkout main
   git merge integration
   git tag v0.2.0
```

### Commit Message Convention

```
<type>: <subject>

<body>

<footer>

Types:
- feat: New feature
- fix: Bug fix
- refactor: Code restructuring
- docs: Documentation
- test: Test addition/modification
- chore: Build, CI, dependencies

Body:
- What changed
- Why it changed
- AI assistance: [Claude|Codex|Copilot|NotebookLM]
- Human review: [reviewer]

Footer:
- Closes: #[issue number]
- Breaking-change: [description if applicable]
```

**Example**:

```
feat: add taxonomy hierarchy traversal endpoint

Added GET /api/taxa/{id}/hierarchy endpoint returning complete 
lineage from kingdom to species. Handles orphaned parent_id 
records gracefully with null-safe navigation.

- Added ITaxonRepository.GetHierarchyAsync() method
- Added TaxonService.GetHierarchyAsyncAsync() with depth limit
- Added TaxonHierarchyDto for response serialization
- Added comprehensive unit tests for edge cases

AI-assisted: Codex (service generation), Copilot (refactoring)
Human review: @alice-architect
Validation: @claude-model (architecture review)

Closes #245
```

## Example Real Workflow: Adding Taxonomy Search Endpoint

### Phase 1: Architecture Planning (Claude)

**Trigger**: Feature request "Users need to search taxa by scientific or common name"

**Developer Action**:

```
Prompt to Claude:

"I need to add a search endpoint to the BirdTaxonomy API.

Requirements:
- Search taxa by scientific name pattern or common name pattern
- Support case-insensitive matching
- Return paginated results (max 100)
- Include related species info in response

Context:
[Paste Docs/API/endpoints.md]
[Paste Docs/Database/schema.md]
[Paste TaxonRepository interface]

Please provide:
1. Architecture design (classes, interfaces, layers)
2. Database query strategy (avoid N+1 queries)
3. DTO specification
4. HTTP contract (endpoint, parameters, responses)
5. Exception handling strategy
6. Implementation order"
```

**Claude Output**:

```
ARCHITECTURE DESIGN:

Layer: Domain
  - SpecificationPattern (SearchSpecification<Taxon>)

Layer: Application
  - ITaxonSearchService interface
  - TaxonSearchService implementation
  - TaxonSearchResultDto

Layer: Controllers
  - GET /api/taxa/search endpoint on TaxaController

DATA ACCESS STRATEGY:
- Use EF Core's .Where() with pattern matching
- Include rank information: .Include(t => t.Rank)
- Project to DTO to avoid loading full entity graph
- Limit results at query level

DTO SPECIFICATION:
public class TaxonSearchResultDto
{
    public int Id { get; set; }
    public string ScientificName { get; set; }
    public string CommonName { get; set; }
    public string RankName { get; set; }
}

HTTP CONTRACT:
GET /api/taxa/search?q=felis&type=scientific&page=1&pageSize=20

Parameters:
  - q (required): search pattern, min 2 chars
  - type (optional): 'scientific' | 'common', default 'scientific'
  - page (optional): int >= 1, default 1
  - pageSize (optional): int 1-100, default 20

Response 200:
{
  "data": [ { TaxonSearchResultDto items } ],
  "pagination": {
    "currentPage": 1,
    "pageSize": 20,
    "totalCount": 456,
    "totalPages": 23
  }
}

[continues with exception handling, edge cases...]
```

**Validation Checkpoint 1**: Architecture review approved ✓

### Phase 2: Implementation (Codex)

**Trigger**: Architecture approved by lead architect

**Developer Action**:

```
Prompt to Codex:

"Implement a search service for the BirdTaxonomy API.

Follow this architecture:

LAYER: Application/Services
class TaxonSearchService implements ITaxonSearchService
- Constructor: inject ITaxonRepository
- Method: Task<SearchResultsDto<TaxonSearchResultDto>> 
  SearchAsync(string pattern, string type, int page, int pageSize)

REQUIREMENTS:
1. Validate parameters (pattern minimum 2 chars, pageSize 1-100)
2. Use repository to search taxa
3. Apply pagination at query level
4. Return count for pagination
5. Throw SearchException for invalid parameters
6. Map Taxon entities to TaxonSearchResultDto

DATABASE SCHEMA (for context):
[Paste schema.md excerpt with taxon, rank, species_info tables]

NAMING CONVENTIONS:
[Paste naming conventions from Docs/Code/conventions.md]

EXISTING PATTERNS:
[Paste similar service: BirdService.cs from repo]

Generate:
1. ITaxonSearchService interface
2. TaxonSearchService implementation
3. SearchResultsDto class
4. Unit tests (xUnit)"
```

**Codex Output**:

```
Generated three classes:
1. ITaxonSearchService.cs
2. TaxonSearchService.cs (implementation)
3. TaxonSearchServiceTests.cs (unit tests)

All classes generated with:
- Complete XML documentation
- Async/await patterns
- Proper null handling
- Exception handling (ArgumentException, SearchException)
- DI registration template for Program.cs
```

**Developer Action**: Copy Codex output to project files

### Phase 3: Refinement (Copilot)

**Trigger**: Code in IDE, developer refining generated code

**Copilot Interactions** (automatic):

1. Developer types incomplete method in controller:
   ```csharp
   [HttpGet("search")]
   public async Task<ActionResult<SearchResultsDto<TaxonSearchResultDto>>> Search(
       [FromQuery] string q,
       [FromQuery] string type = "scientific",
       [FromQuery] int page = 1,
       [FromQuery] int pageSize = 20)
   {
       // [Copilot suggests implementation]
   }
   ```

2. Copilot suggests:
   ```csharp
   try
   {
       var results = await _service.SearchAsync(q, type, page, pageSize);
       return Ok(results);
   }
   catch (SearchException ex)
   {
       return BadRequest(new { error = ex.Message });
   }
   ```

3. Developer accepts suggestion; makes minor adjustments

### Phase 4: Code Review

**Participants**: Senior developer (code review), Architect (optional)

**Review Checklist**:

- [x] Compiles without errors
- [x] Tests pass (100% pass rate)
- [x] Layer boundaries respected (service doesn't access DbContext)
- [x] Repository pattern used (no direct database access)
- [x] Parameter validation complete
- [x] Error handling for all exception cases
- [x] N+1 query check: EF Core logging shows single query
- [x] DTOs used correctly at API boundary
- [x] XML documentation complete
- [x] HTTP status codes correct (200, 400, 404)
- [x] Pagination formula correct (skip = (page-1)*pageSize)

**Result**: Approved with suggestions for minor improvements

### Phase 5: Integration Testing

**Trigger**: Code review approved

**Manual Tests**:

1. Valid search by scientific name
2. Valid search by common name
3. Case-insensitive matching verification
4. Pagination works (page 2, page 3)
5. PageSize limit enforced (request 101, get 100)
6. Empty results (search for non-existent species)
7. Validation error (pattern < 2 chars)
8. Database error handling

**Automated Tests**:

```
Run: dotnet test

Results:
TaxonSearchServiceTests.cs: 12 tests, all passed
TaxonSearchControllerTests.cs: 8 tests, all passed
Integration tests: 6 tests, all passed

Total: 26 tests passed
```

### Phase 6: Claude Validation Review (Optional)

**Trigger**: Complex change; optional validation

```
Prompt to Claude:

"Please review this new search endpoint implementation.

Controller method: [paste controller code]
Service implementation: [paste service code]
Tests: [paste test code]

Architecture docs: [paste relevant architecture docs]
Database schema: [paste schema]

Please validate:
1. Is layer separation correct?
2. Are there performance concerns?
3. Are there security issues?
4. Is documentation accurate?
5. Are there improvements?"
```

**Claude Response**:

```
VALIDATION REPORT: APPROVED with suggestions

Layer Separation: ✓ CORRECT
- Controller delegates to service
- Service uses repository (doesn't access DbContext)
- DTO mapping happens in service

Performance: ✓ GOOD
- Single database query verified
- Pagination limits result set
- Index on taxon(name) should exist (recommendation)

Security: ✓ SAFE
- Input parameters validated
- Parameter injection not possible
- No sensitive data in response

Documentation: ✓ COMPLETE
- XML comments present
- Parameter constraints documented
- Exception conditions documented

Suggestions:
1. Consider adding common_name index for better search performance
2. Document timeout behavior for very broad searches
3. Add rate limiting if search becomes popular
```

### Phase 7: Merge to develop

**Command**:

```bash
git checkout develop
git merge feature/add-taxonomy-search
git push origin develop
```

**Commit Message**:

```
feat: add taxonomy search endpoint

Adds GET /api/taxa/search endpoint supporting scientific and 
common name pattern matching with pagination. Handles legacy 
orphaned records gracefully.

- Added ITaxonSearchService interface
- Implemented TaxonSearchService in Application layer
- Added TaxonSearchResultDto with required fields
- Generated 26 unit/integration tests, all passing
- Validated against database constraints
- No N+1 queries detected

AI-assisted: Codex (implementation), Copilot (refinement)
Human review: @bob-senior-dev
Validation: @claude-model (optional review)

Closes #342
```

### Phase 8: Documentation Update (NotebookLM)

**Trigger**: Pre-release preparation

**Developer Action**:

Create NotebookLM project with source documents:

- Docs/API/endpoints.md (current endpoints)
- Docs/Database/schema.md
- Docs/Architecture/clean-architecture.md
- New: controllers/TaxaController.cs (updated with search)

**Query NotebookLM**:

```
"Based on these architecture and API documentation files,
generate a comprehensive training guide for new developers
explaining the taxonomy search feature. Include:

1. What the search endpoint does
2. How it fits in the architecture
3. Example API requests and responses
4. How the service uses the repository pattern
5. How to extend the search (add new fields, filters)"
```

**NotebookLM Output**:

Generated comprehensive training document with:
- Diagram of request flow
- Architecture explanation
- Code examples
- Extension points
- Links to source documentation

**Developer Action**:

```bash
# Commit training material
git checkout develop
git add Docs/Training/search-endpoint-guide.md
git commit -m "docs: add search endpoint training guide

Generated via NotebookLM synthesis of architecture and 
API documentation. Provides new developer walkthrough of
search feature implementation."
```

### Phase 9: Release

**Timeline**:

```
develop branch → integration branch → tag v0.3.0-rc1
→ testing (48 hours) → 
main branch → tag v0.3.0 (production release)
```

**Release Notes**:

```
## v0.3.0 - Taxonomy Search

### New Features

- **Taxonomy Search Endpoint**: GET /api/taxa/search
  - Search by scientific or common name
  - Case-insensitive pattern matching
  - Paginated results (default 20, max 100)
  - Example: GET /api/taxa/search?q=felis&type=scientific&page=1

### Technical Details

- Implemented via Application/TaxonSearchService
- Single optimized database query (no N+1)
- Handles legacy orphaned records gracefully
- Full test coverage (26 tests)

### Migration Notes

No database migrations required.

### Contributors

Implemented with AI assistance (Codex, Copilot, Claude)
Reviewed and validated by: @bob-senior-dev, @alice-architect
```

## Summary: Agents in Production Workflow

| Phase | Agent | Output | Validation |
|-------|-------|--------|-----------|
| Design | Claude | Architecture specification | Architect review |
| Implementation | Codex | Service, controller, tests | Code review |
| Refinement | Copilot | Polished code, tests | Developer acceptance |
| Quality | Human | Code review, testing | All tests pass |
| Validation | Claude (optional) | Architecture review report | Lead architect |
| Documentation | NotebookLM | Training materials | New dev testing |
| Integration | Git | Versioned, tested code | CI/CD pipeline |
| Release | Human | Production deployment | Release manager |

## Conclusion

The multi-agent workflow leverages each AI model's strengths while maintaining human oversight and validation at critical checkpoints. By establishing clear roles, artifact flows, and validation criteria, the BirdTaxonomy API team achieves rapid development velocity without sacrificing code quality, architectural integrity, or domain accuracy.

Success depends on:

1. **Clear Role Definition**: Each agent knows its scope
2. **Explicit Context**: All context provided to agents
3. **Human Validation**: Every artifact reviewed before merge
4. **Serialized Workflow**: Agents invoked in logical sequence
5. **Continuous Learning**: Failures inform prompt and context updates

The workflow scales with team size and project complexity. As the team grows, formalize checkpoints and automate validation where possible (code analysis, test execution, documentation consistency).