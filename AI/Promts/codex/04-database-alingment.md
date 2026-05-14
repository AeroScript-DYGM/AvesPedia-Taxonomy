# Database Alignment Prompt

Analyze the current ORM and SQL Server schema.

Validate:

- Entity mappings
- Foreign keys
- Fluent API configuration
- DTO alignment
- Query behavior

Current schema:

- rank
- taxon
- species_info

Relationships:

- rank 1:N taxon
- taxon 1:0..1 species_info

Rules:

- Never rename physical tables.
- Never flatten existing relationships.
- Preserve SQL Server compatibility.
- Preserve HTTP 500 behavior on DB communication failures.

Return:

- Misalignments
- Risks
- Exact corrections
- File diffs