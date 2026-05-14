# 03 - Revisar Dependencies y Inyección

## 🔴 **PROBLEMAS DE DEPENDENCIAS**

### 1. **IGenericRepository Sin Uso Directo**
```csharp
// Application/Contracts/Repositories/IGenericRepository.cs
public interface IGenericRepository<TEntity> where TEntity : class
{
    Task<IReadOnlyCollection<TEntity>> GetAllAsync(...);
    Task<TEntity?> GetByIdAsync(...);
    // ...
}

// Pero en el código:
// Program.cs NO registra IGenericRepository<T>
// Usa: ITaxonRepository y IRankRepository (que heredan de IGenericRepository)

// ITaxonRepository nunca se usa directamente
public interface ITaxonRepository : IGenericRepository<Taxon>
{
    Task<IReadOnlyCollection<Taxon>> GetAllWithRelationsAsync(...);
    // ... métodos especializados
}
```

**Impacto**: `IGenericRepository` está definido pero subutilizado. Confunde.

---

### 2. **Conversión Insegura en Service**
```csharp
// TaxonomiaConsultaService.cs - CrearTaxonAsync
await ValidarRankAsync((int)request.RankId, cancellationToken);  // ✗ CAST SIN VALIDAR
var duplicado = await _unitOfWork.Taxones.ExistsByNameAndRankAsync(
    nombre, 
    (int)request.RankId,  // ✗ Si RankId es null → InvalidOperationException
    cancellationToken: cancellationToken
);
```

**Impacto**: Si `request.RankId` es null, crash en runtime. No se valida antes del cast.

---

### 3. **ConservationStatusNormalizer No Encontrado**
```csharp
// Se usa en TaxonomiaConsultaService
ConservationStatusNormalizer.Normalize(request.SpeciesInfo.EstadoConservacion)

// Pero está declarado en:
// Domain/ConservationStatusNormalizer.cs
// ¿Está en el namespace correcto? ¿Se importa?
```

**Impacto**: Posible `using` faltante o clase no registrada en DI.

---

## ✅ **ACCIONES**

1. **Remover `IGenericRepository`** o usarlo explícitamente
2. **Validar `request.RankId` antes del cast**: 
   ```csharp
   if (!request.RankId.HasValue)
       throw new ArgumentException("RankId es requerido");
   var rankId = request.RankId.Value;
   ```
3. **Verificar `ConservationStatusNormalizer`** está en namespace correcto
4. **Agregar validación centralizada** en DTO o en middleware
