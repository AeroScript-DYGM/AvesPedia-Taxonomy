# 01 - Detectar Inconsistencias Críticas

## 🔴 **PROBLEMAS DETECTADOS**

### 1. **Nullability Inconsistency en Taxon.RankId**
```csharp
// Domain/Entities/Taxon.cs
public int? RankId { get; set; }  // ✗ NULLABLE

// Persistence/Configurations/TaxonConfiguration.cs
builder.Property(x => x.RankId).HasColumnName("rankid").IsRequired();  // ✓ REQUIRED
```
**Impacto**: `RankId` puede ser null en C# pero la DB lo requiere. Riesgo de `InvalidOperationException` en runtime.

---

### 2. **Entity Naming vs DTO Naming Mismatch**
```csharp
// Entidad
public string? Name { get; set; }

// DTO
public string? NombreCientifico { get; init; }
```
**Impacto**: Confusión al mapear. `Name` debería ser `NombreCientifico` o viceversa.

---

### 3. **RankDto Nullability Inconsistency**
```csharp
// Domain/Entities/Rank.cs
public int Id { get; set; }  // ✓ NO NULLABLE

// Application/DTOs/RankDto.cs
public int? RangoId { get; init; }  // ✗ NULLABLE
```
**Impacto**: El cliente recibe `null` para un ID que nunca puede ser null en DB.

---

### 4. **DbContext Comentario Incompleto**
```csharp
// Persistence/BirdTaxonomyDbContext.cs
// "¿TIENEN PROBLEMAS DE RELACION CON LA DE BASE DE DATOS?"
```
**Impacto**: Sugiere duda sobre el esquema. Necesita aclaración.

---

## ✅ **ACCIONES RECOMENDADAS**

1. Hacer `Taxon.RankId` **no nullable** → `public int RankId { get; set; }`
2. Renombrar DTO properties para alinear con convención o entidad
3. Remover nullable en `RankDto.RangoId` y `RankDto.NombreRango` (si vienen de DB)
4. Resolver comentario sobre relaciones en DbContext
