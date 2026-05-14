# Entity Relationship Model

## Overview

BirdTaxonomy uses an existing SQL Server legacy schema.

The API and ORM adapt to the physical database instead of generating a new schema.

The current database contains three core tables:

- rank
- taxon
- species_info

---

## Entity Relationship Diagram

![ER Diagram](../Images/er/birdtaxonomy-er-api.png)

---

## Relationships

### rank → taxon

Relationship:

1:N

Meaning:

One taxonomy rank can contain multiple taxa.

Example:

- Species
    - Falco peregrinus
    - Gallus gallus
    - Anas platyrhynchos

Foreign key:

```sql
taxon.rankid → rank.ID
```

---

### taxon → species_info

Relationship:

1:0..1

Meaning:

A taxon may optionally contain species-level biological metadata.

Foreign key:

```sql
species_info.taxonid → taxon.ID
```

---

## Entity Mapping

| Domain Entity | Physical Table |
|---|---|
| Rank | rank |
| Taxon | taxon |
| SpeciesInfo | species_info |

---

## ORM

Entity Framework Core maps:

- Rank
- Taxon
- SpeciesInfo

Through:

- BirdTaxonomyDbContext
- Fluent API configurations

---

## Design Rule

The ORM must always preserve the legacy schema.

Forbidden:

- Renaming tables
- Renaming columns
- Flattening relationships
- Replacing foreign keys