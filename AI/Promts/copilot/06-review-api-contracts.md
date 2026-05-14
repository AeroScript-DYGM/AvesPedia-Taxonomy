# 06 - Revisar API Contracts y Respuestas

## ✅ **LO QUE ESTÁ BIEN**

```csharp
[HttpGet("rangos")]
[ProducesResponseType(typeof(IReadOnlyCollection<RankDto>), StatusCodes.Status200OK)]
[ProducesResponseType(StatusCodes.Status500InternalServerError)]
public async Task<ActionResult<IReadOnlyCollection<RankDto>>> GetRangosAsync(CancellationToken cancellationToken)
{
    // ✓ ProducesResponseType documentado
    // ✓ CancellationToken soportado
    // ✓ Async/await
}
```

---

## 🔴 **PROBLEMAS EN CONTRATOS**

### 1. **Falta Validación en Request**
```csharp
[HttpPost("taxones")]
public async Task<ActionResult<TaxonDetalleDto>> CreateTaxonAsync(
    [FromBody] CrearTaxonRequestDto request,  // ← SIN [Validate]
    CancellationToken cancellationToken)
{
    try {
        var response = await _taxonomiaConsultaService.CrearTaxonAsync(request, cancellationToken);
        // ...
    }
}
```

**Problema**: El controller NO valida el DTO. La validación está en el service.

---

### 2. **Error Handling Incompleto**
```csharp
catch (ArgumentException exception)
{
    return BadRequest(new { message = exception.Message });
}
catch (InvalidOperationException exception)
{
    return Conflict(new { message = exception.Message });
}
catch (DbUpdateException exception)
{
    return MapearDbUpdateException(exception);
}
// ✗ ¿Qué pasa si lanza otra excepción? Cae en ExceptionHandler global
```

---

### 3. **ProducesResponseType Incompleto**
```csharp
[HttpPost("taxones")]
[ProducesResponseType(typeof(TaxonDetalleDto), StatusCodes.Status201Created)]
[ProducesResponseType(StatusCodes.Status400BadRequest)]
[ProducesResponseType(StatusCodes.Status409Conflict)]
[ProducesResponseType(StatusCodes.Status500InternalServerError)]
// ✗ Falta especificar estructura de error 400/409
```

---

### 4. **POST sin Validación de Entrada**
```csharp
// Client envía:
{
  "nombre": "",           // ✗ Empty string
  "rankId": null,        // ✗ Null (sin validar antes del cast)
  "speciesInfo": null
}

// Service no rechaza hasta después:
var nombre = ValidarNombre(request.Nombre);  // Solo en service
```

**Impacto**: API acepta requests inválidos, valida tarde.

---

### 5. **Response Structure Inconsistente**
```csharp
// 400 BadRequest:
new { message = exception.Message }

// 409 Conflict:
new { message = exception.Message }

// 500 InternalServerError:
new {
    message = "...",
    detail = detalle  // ← ¿Por qué detail aquí y no en otros?
}
```

---

## ✅ **RECOMENDACIONES**

1. **Agregar [ApiController] validation**:
   ```csharp
   [ApiController]  // ✓ Ya está
   [Route("api/[controller]")]
   // Esto auto-valida DTOs con data annotations
   ```

2. **Agregar Data Annotations a DTOs**:
   ```csharp
   public sealed class CrearTaxonRequestDto
   {
       [Required(ErrorMessage = "Nombre es requerido")]
       [StringLength(100, MinimumLength = 3)]
       public string Nombre { get; init; }
       
       [Required(ErrorMessage = "RankId es requerido")]
       public int RankId { get; init; }  // NO nullable
   }
   ```

3. **Estandarizar Response Structure**:
   ```csharp
   public class ErrorResponse
   {
       public string Message { get; set; }
       public string? Detail { get; set; }
       public Dictionary<string, string[]>? Errors { get; set; }
   }
   ```

4. **Documentar en Swagger**: 
   ```csharp
   [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
   ```
