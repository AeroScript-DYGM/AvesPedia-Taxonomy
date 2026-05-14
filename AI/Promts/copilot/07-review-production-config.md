# 07 - Revisar Configuración de Producción

## 🔴 **PROBLEMAS CRÍTICOS DE SEGURIDAD**

### 1. **Credentials en Plain Text en appsettings.json**
```json
{
  "ConnectionStrings": {
    "BirdTaxonomyDb": "Server=<db-server>;Database=<db-name>;User Id=<db-user>;Password=<db-password>;TrustServerCertificate=True;"
  }
}
```

**RIESGO CRÍTICO**: 
- ✗ Contraseña visible en control de versiones
- ✗ Expuesta en logs
- ✗ Accesible a cualquiera con acceso al servidor
- ✗ Violación de OWASP/compliance

---

### 2. **CORS Hardcodeado**
```csharp
policy.WithOrigins(
    "http://localhost:3000",
    "https://localhost:3000",
    "http://localhost:4200",
    "https://localhost:4200",
    "http://localhost:5001",
    "https://localhost:5001",
    "https://yourdomain.com",
    "http://yourdomain.com")  // ← Publicado, no configurable
```

**PROBLEMA**:
- ✗ Sin HTTPS solo (HTTP permitido)
- ✗ Si cambia el dominio en prod, hay que recompilar
- ✗ No configurable por environment

---

### 3. **Logging Excesivo en Desarrollo, Insuficiente en Producción**
```csharp
builder.Logging.SetMinimumLevel(
    builder.Environment.IsDevelopment() ? LogLevel.Debug : LogLevel.Information
);
```

**Pero en appsettings.json**:
```json
"Logging": {
  "LogLevel": {
    "Default": "Information",  // ← Igual en dev y prod
    "Microsoft.AspNetCore": "Warning"
  }
}
```

**INCONSISTENCIA**: Program.cs dice Debug en dev, pero appsettings lo sobrescribe.

---

### 4. **EnableSensitiveDataLogging en Desarrollo NO CONTROLADO**
```csharp
if (builder.Environment.IsDevelopment())
    options.EnableSensitiveDataLogging();  // ✓ Bien limitado a dev
```

✓ Esto SÍ está bien hecho.

---

### 5. **ErrorHandler Genérico Expone Detalles**
```csharp
app.UseExceptionHandler(exceptionHandlerApp =>
{
    exceptionHandlerApp.Run(async context =>
    {
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        await context.Response.WriteAsJsonAsync(new ProblemDetails
        {
            Status = StatusCodes.Status500InternalServerError,
            Title = "Resistencia de comunicacion o persistencia",
            Detail = "La API no pudo completar la operacion contra SQL Server."
        });
    });
});
```

**MEJORA**: En prod, `Detail` no debería exponer que es SQL Server.

---

### 6. **Retry Policy Sin Límites**
```csharp
options.UseSqlServer(connectionString, sqlOptions =>
{
    sqlOptions.EnableRetryOnFailure();  // ← ¿Cuántos intentos? ¿Cuál es el timeout?
});
```

**RIESGO**: Retry indefinido puede causar cascada de fallos.

---

## ✅ **ACCIONES INMEDIATAS**

### 1. **Usar User Secrets (Desarrollo)**
```bash
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:BirdTaxonomyDb" "Server=..."
```

### 2. **Usar Azure Key Vault / AWS Secrets Manager (Producción)**
```csharp
if (!builder.Environment.IsDevelopment())
{
    var kvUri = new Uri(builder.Configuration["KeyVault:Uri"]);
    builder.Configuration.AddAzureKeyVault(
        kvUri,
        new DefaultAzureCredential());
}
```

### 3. **Separar CORS por Environment**
```csharp
var corsOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>();

policy.WithOrigins(corsOrigins)
    .AllowAnyHeader()
    .AllowAnyMethod();
```

**appsettings.json**:
```json
{
  "Cors": {
    "AllowedOrigins": ["http://localhost:3000", "https://localhost:4200"]
  }
}
```

**appsettings.Production.json**:
```json
{
  "Cors": {
    "AllowedOrigins": ["https://yourdomain.com"]
  }
}
```

### 4. **Configurar Retry Policy Explícitamente**
```csharp
sqlOptions.EnableRetryOnFailure(
    maxRetryCount: 3,
    maxRetryDelaySeconds: 5,
    errorNumbersToAdd: null);
```

### 5. **Sanitizar Error Messages en Producción**
```csharp
if (builder.Environment.IsProduction())
{
    exceptionHandlerApp.Run(async context =>
    {
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        await context.Response.WriteAsJsonAsync(new ProblemDetails
        {
            Status = StatusCodes.Status500InternalServerError,
            Title = "Internal Server Error",
            // ✗ NO incluir Detail en producción
        });
    });
}
```

---

## 📋 **CHECKLIST PRE-PRODUCCIÓN**

- [ ] Credenciales en Azure Key Vault / AWS Secrets Manager
- [ ] CORS configurado por environment
- [ ] HTTPS ONLY (no HTTP)
- [ ] Logging estructurado (Serilog)
- [ ] Error messages sanitizados
- [ ] Retry policy con límites
- [ ] Health checks configurados
- [ ] Rate limiting implementado
- [ ] Certificados SSL válidos
- [ ] Backups y DR plan
