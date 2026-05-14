# Database Schema Notes

## Database Engine

Production:

SQL Server

Hosting:

MonsterASP.NET


---


## Physical Tables

## rank

Purpose:

Stores taxonomy rank catalog.

Columns:

| Column | Type | Notes |
|---|---|---|
| ID | int | Primary Key |
| name_ | varchar(100) | Rank name |

Examples:

- Clado
- Orden
- Familia
- Género
- Especie

---

## taxon

Purpose:

Stores scientific taxa.

Columns:

| Column | Type | Notes |
|---|---|---|
| ID | int | Primary Key |
| name_ | varchar(150) | Scientific name |
| rankid | int | Foreign Key |

Relationship:

```txt
taxon.rankid → rank.ID
```

Important note:

`taxon.ID` is NOT identity.

ID generation is manual:

```txt
MAX(ID)+1
```

---

## species_info

Purpose:

Stores biological metadata.

Columns:

| Column | Type |
|---|---|
| taxonid | int |
| commun_name | varchar(100) |
| description_ | varchar(200) |
| domesticated | bit |
| conservation_status | varchar(50) |
| geographic_location | varchar(150) |

Relationship:

```txt
species_info.taxonid → taxon.ID
```

---

## ORM Rules

Entity Framework Core must use:

- Fluent API
- Explicit relationships
- Explicit delete behavior

---

## Error Handling

If SQL Server communication fails:

API must return:

```http
HTTP 500
```

---

## Migration Strategy

Current strategy:

Database First adaptation.

Not:

Code First schema generation.

Reason:

BirdTaxonomy works over an existing scientific legacy schema.