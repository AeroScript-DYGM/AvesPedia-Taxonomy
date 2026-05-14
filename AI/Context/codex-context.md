# BirdTaxonomyDbContext - Contexto para Codex / ChatGPT

Nota historica:
- Una iteracion anterior del proyecto apunto temporalmente a SQLite.
- La configuracion vigente del entorno usa SQL Server LocalDB con base de datos `Aves`.

Ubicacion del archivo DbContext en el proyecto:
- BirdTaxonomy.API/Persistence/BirdTaxonomyDbContext.cs
- Clase: BirdTaxonomy.API.Persistence.BirdTaxonomyDbContext

Como se registra en el proyecto (Program.cs):
- Archivo: BirdTaxonomy.API/Program.cs
- Clave de configuracion esperada en appsettings.json: ConnectionStrings:BirdTaxonomyDb
- Proveedor vigente: `UseSqlServer(...)`

Servidor, base de datos y archivos detectados:
- Server: (localdb)\\MSSQLLocalDB
- Database: Aves
- MDF: C:\\Users\\quewt\\Aves.mdf
- LDF: C:\\Users\\quewt\\Aves_log.ldf

Cadena de conexion vigente:

Server=(localdb)\\MSSQLLocalDB;Database=Aves;Trusted_Connection=True;MultipleActiveResultSets=true

Esquema fisico actual que el ORM debe respetar:
- `rank`
- `taxon`
- `species_info`

Relaciones actuales:
- `taxon.rankid -> rank.ID`
- `species_info.taxonid -> taxon.ID`

Mapeo actual esperado en ORM:
- `Rank` -> tabla `rank`
- `Taxon` -> tabla `taxon`
- `SpeciesInfo` -> tabla `species_info`

API actual esperada sobre este contexto:
- `GET /api/taxonomia/rangos`
- `GET /api/taxonomia/taxones`
- `GET /api/taxonomia/taxones/{id}`
- `POST /api/taxonomia/taxones`
- `PUT /api/taxonomia/taxones/{id}`
- `DELETE /api/taxonomia/taxones/{id}`

Resistencia de comunicacion:
- Si la API no puede comunicarse con SQL Server LocalDB, la respuesta documentada y entregada por runtime debe ser HTTP 500.

Paquetes NuGet necesarios:
- Microsoft.EntityFrameworkCore.SqlServer
- Microsoft.EntityFrameworkCore.Design (opcional para herramientas de diseno)

Nota de trabajo:
- Este contexto ya no debe asumir una jerarquia fisica `Clado -> Superorden -> Orden -> Familia -> Especie -> Ave`.
- Esa interpretacion puede existir a nivel de negocio futuro, pero el `DbContext` actual debe alinearse primero con las tablas reales existentes.
- Dato historico: la API comenzo solo con operaciones GET y despues se extendio para soportar POST, PUT y DELETE sobre el esquema legado.

Proposito de este archivo:
Proveer a Codex/ChatGPT la informacion minima y directa para localizar y entender la configuracion real del DbContext, la base `Aves`, sus tablas actuales y la expectativa de error HTTP 500 ante fallos de comunicacion.
