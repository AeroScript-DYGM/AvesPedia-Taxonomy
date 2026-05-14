# 04 - Revisar Clean Architecture

## ✅ **LO QUE ESTÁ BIEN**

```
BirdTaxonomy.API/
├── Domain/                          ✓ Entidades puras (no depende de nada)
│   └── Entities/ (Taxon, Rank, SpeciesInfo)
├── Application/                     ✓ Servicios, DTOs, Contratos
│   ├── Contracts/ (Interfaces)
│   ├── DTOs/
│   └── Services/
├── Infrastructure/                  ✓ Implementaciones (Repos, UnitOfWork)
│   └── Repositories/
├── Persistence/                     ✓ DbContext y Configurations
├── Controllers/                     ✓ API Layer
└── Program.cs                       ✓ DI Configuration
```

---

## 🟡 **PROBLEMAS EN LA ARQUITECTURA**

### 1. **Mezcla de Responsabilidades en Service**
```csharp
// TaxonomiaConsultaService hace DEMASIADO:
public async Task<TaxonDetalleDto> CrearTaxonAsync(...)
{
    var nombre = ValidarNombre(...);              // ✓ Validación
    await ValidarRankAsync(...);                  // ✓ Validación
    ValidarSpeciesInfo(...);                      // ✓ Validación
    var duplicado = await _unitOfWork.Taxones.ExistsByNameAndRankAsync(...);  // ✓ Consulta
    var taxon = new Taxon { ... };                // ✗ Creación (Dominio)
    taxon.SpeciesInfo = CrearSpeciesInfo(...);    // ✗ Creación (Dominio)
    // ...más lógica
}
```

**Problema**: Debería haber un **Domain Service** o **Factory** para crear entidades.

---

### 2. **Lógica de Validación en Service, No en Domain**
```csharp
// Está en TaxonomiaConsultaService:
if (duplicado) throw new InvalidOperationException("Ya existe...");

// Debería estar en Domain:
// public class Taxon { 
//   public static void ValidarDuplicado(...) 
// }
```

---

### 3. **GetNextIdAsync es Antipatrón**
```csharp
var taxon = new Taxon {
    Id = await _unitOfWork.Taxones.GetNextIdAsync(cancellationToken),  // ⚠️
    // ...
}
```

**Razón**: La tabla `taxon` tiene `ValueGeneratedNever()`, sin IDENTITY.  
**Riesgo**: Race condition en concurrencia, no escalable.

---

## ✅ **RECOMENDACIONES**

1. **Crear Domain Services** para lógica transversal
2. **Mover validación de negocio** a Domain
3. **Refactorizar GetNextIdAsync** → Usar IDENTITY o distribuir IDs (GUID)
4. **Separar DTO mapping** en Profile (Automapper) o Mapper class
