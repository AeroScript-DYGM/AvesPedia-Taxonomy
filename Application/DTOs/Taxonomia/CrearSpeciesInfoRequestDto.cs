namespace BirdTaxonomy.API.Application.DTOs.Taxonomia;


// aver esto debe comunicarse bien con la base de datos, ahora yo tengo en especies de la base de datos esto:
//taxonid,commun_name,description_,domesticated,status_conversation,geographic_location.
//se me hace raro y puede que no lea datos de mi api, POR ESO TAL VEZ HAY resistencia en mi api

public sealed class CrearSpeciesInfoRequestDto
{
    public string NombreComun { get; init; } = string.Empty;
    public string? Descripcion { get; init; }
    public bool Domesticada { get; init; }
    public string? EstadoConservacion { get; init; }
    public string? UbicacionGeografica { get; init; }
}
