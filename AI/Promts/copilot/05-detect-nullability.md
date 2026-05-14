# 05 - Detectar Problemas de Nullability

## 🔴 **NULL REFERENCES PELIGROSOS**

### 1. **Cast Inseguro en CrearTaxonAsync**
```csharp
// TaxonomiaConsultaService.cs
await ValidarRankAsync((int)request.RankId, cancellationToken);  // ← CRASH si null
```

**Escenario**:
```json
POST /api/taxonomia/taxones
{
  "nombre": "Columba palumbus",
  "rankId": null,
  "speciesInfo": null
}
```

**Resultado**: `System.InvalidOperationException: Nullable object must have a value`

---

### 2. **GetByIdWithRelationsAsync Puede Retornar Null**
```csharp
// TaxonomiaConsultaService.cs
var creado = await _unitOfWork.Taxones.GetByIdWithRelationsAsync(taxon.Id, cancellationToken);
return MapearDetalle(creado ?? taxon);  // ✓ Tiene null coalescing

// Pero:
var actualizado = await _unitOfWork.Taxones.GetByIdWithRelationsAsync(id, cancellationToken);
return actualizado is null ? null : MapearDetalle(actualizado);  // ✓ Bien
```

**Inconsistencia**: Un lugar usa `??`, otro usa `is null`.

---

### 3. **SpeciesInfo Nullable Sin Validar**
```csharp
// CrearTaxonRequestDto
public CrearSpeciesInfoRequestDto? SpeciesInfo { get; init; }

// Pero en el service:
if (request.SpeciesInfo is null)
    taxon.SpeciesInfo = null;
else if (taxon.SpeciesInfo is null)
    taxon.SpeciesInfo = new SpeciesInfo { ... };
else
    taxon.SpeciesInfo.CommonName = request.SpeciesInfo.NombreComun.Trim();  // ← .Trim() en string?
```

**Riesgo**: `NombreComun` puede ser null si se deserializa mal.

---

### 4. **RankDto Nullability Falsa**
```csharp
public sealed class RankDto
{
    public int? RangoId { get; init; }          // ← NUNCA será null
    public string? NombreRango { get; init; }   // ← ¿Y si Rank.Name es null en DB?
}

// Pero en Program.cs:
builder.Services.AddScoped<IRankRepository, RankRepository>();

// Y en Configuration:
builder.Property(x => x.Name).IsRequired();  // ← REQUIRED en DB
```

---

## ✅ **FIXES PRIORITARIOS**

1. **Validar RankId antes del cast**:
   ```csharp
   if (!request.RankId.HasValue)
       throw new ArgumentException("RankId es requerido");
   ```

2. **Normalizar null handling**:
   - Usar siempre `is null` o `?? default`
   - No mezclar

3. **Remover nullability innecesaria**:
   ```csharp
   public int RangoId { get; init; }      // No int?
   public string NombreRango { get; init; }  // No string?
   ```

4. **Validar strings después de Trim()**:
   ```csharp
   var commonName = request.SpeciesInfo?.NombreComun?.Trim();
   if (string.IsNullOrWhiteSpace(commonName))
       throw new ArgumentException("NombreComun es requerido");
   ```
