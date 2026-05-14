# 02 - Revisar DTO Contracts

## **PROBLEMAS EN MAPEO DTO ↔ ENTIDAD**

### 1. **Propiedades Inconsistentes entre Capas**
```
Taxon Entity:          DTO Output:           DTO Input:
- Id                   - TaxonId ✓          - (N/A - auto-gen)
- Name                 - NombreCientifico   - Nombre ✗
- RankId (nullable)    - RangoId (nullable) - RankId
- Rank                 - NombreRango ✓      - (N/A)
- SpeciesInfo          - SpeciesInfo ✓      - SpeciesInfo
```

**Problema**: Mapeo manual, propenso a errores. No hay validación en DTO.

---

### 2. **SpeciesInfo Naming Caos**
```csharp
// Entity
public string CommonName { get; set; }
public string? ConservationStatus { get; set; }

// DTO Output
public string NombreComun { get; init; }
public string? EstadoConservacion { get; init; }

// DTO Input (CrearSpeciesInfoRequestDto)
public string NombreComun { get; init; }
public string? EstadoConservacion { get; init; }

// Pero en Configuration
builder.Property(x => x.CommonName).HasColumnName("commun_name")
builder.Property(x => x.ConservationStatus).HasColumnName("conservation_status")
```

**Impacto**: Triple traducción de nombres (Entity → Config → DTO → DB).

---

### 3. **Falta Validación en DTO de Request**
```csharp
// CrearTaxonRequestDto - SIN VALIDACIÓN
public sealed class CrearTaxonRequestDto
{
    public string Nombre { get; init; } = string.Empty;  // ✗ No [Required]
    public int? RankId { get; init; }                     // ✗ Nullable, sin validar
    public CrearSpeciesInfoRequestDto? SpeciesInfo { get; init; }
}
```

La validación ocurre en el **service**, no en el DTO.

---

## ✅ **SOLUCIONES**

1. **Estandarizar nombres**: Decidir si es `NombreCientifico` o `Name`
2. **Agregar Data Annotations** a DTOs de request
3. **Usar Automapper** o similar para mapeos consistentes
4. **Documentar** el contrato API (OpenAPI/Swagger)
