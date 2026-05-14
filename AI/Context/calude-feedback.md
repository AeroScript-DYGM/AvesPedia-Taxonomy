# BirdTaxonomy.API — Contexto para Codex

## Rol
Eres un agente experto en ASP.NET Core 8, EF Core y SQL Server.
Antes de generar cualquier código: lee el archivo completo, lista todos los archivos afectados y muestra el cambio exacto antes de aplicarlo.

---

## Proyecto
API REST para taxonomía de aves sobre esquema legacy de SQL Server.
- Hosting: MonsterASP.NET (avespedia.runasp.net)
- BD producción: db51089.databaseasp.net / db51089
- BD local: (localdb)\MSSQLLocalDB / Aves

---

## Esquema físico real (NO modificar nombres)
| Tabla | Columna | Tipo |
|---|---|---|
| rank | ID | int PK |
| rank | name_ | varchar(100) |
| taxon | ID | int PK (NO identity, manual MAX+1) |
| taxon | name_ | varchar(150) |
| taxon | rankid | int FK → rank.ID |
| species_info | taxonid | int PK FK → taxon.ID |
| species_info | commun_name | varchar(100) |
| species_info | description_ | varchar(200) |
| species_info | domesticated | bit |
| species_info | conservation_status | varchar(50) |
| species_info | geographic_location | varchar(150) |

---

## Contrato JSON real del frontend

### GET /api/taxonomia/rangos → debe devolver:
```json
[{ "rangoId": 1, "nombreRango": "Especie" }]
```

### GET /api/taxonomia/taxones → debe devolver:
```json
[{
  "taxonId": 1,
  "nombreCientifico": "Falco peregrinus",
  "rangoId": 7,
  "nombreRango": "Especie",
  "speciesInfo": {
    "nombreComun": "Halcón peregrino",
    "descripcion": "...",
    "domesticada": false,
    "estadoConservacion": "LC",
    "ubicacionGeografica": "Global"
  }
}]
```

### POST/PUT /api/taxonomia/taxones ← frontend envía:
```json
{
  "nombre": "Falco peregrinus",
  "rankId": 7,
  "speciesInfo": {
    "nombreComun": "Halcón peregrino",
    "descripcion": "...",
    "domesticada": false,
    "estadoConservacion": "LC",
    "ubicacionGeografica": "Global"
  }
}
```

---

## DTOs actuales con desalineación conocida
```csharp
// RankDto — INCORRECTO, frontend espera rangoId/nombreRango
public sealed class RankDto { public int Id; public string? Nombre; }

// CrearTaxonRequestDto — recibe nombre/rankId (camelCase coincide con frontend)
public sealed class CrearTaxonRequestDto { public string Nombre; public int RankId; public CrearSpeciesInfoRequestDto? SpeciesInfo; }

// TaxonResumenDto — desconocido, revisar antes de editar
```

---

## Problemas activos
1. RankDto devuelve id/nombre → frontend espera rangoId/nombreRango
2. TaxonResumenDto muestra undefined en tabla del frontend
3. appsettings.json en producción puede apuntar a LocalDB en lugar de db51089

---

## Reglas estrictas
1. NUNCA renombrar tablas ni columnas físicas de BD
2. taxon.ID NO es identity — el servicio calcula MAX(ID)+1
3. JSON siempre camelCase
4. Si cambias un DTO actualiza el servicio que lo mapea
5. Ante fallo SQL Server → HTTP 500
6. Si hay duda, pregunta antes de generar