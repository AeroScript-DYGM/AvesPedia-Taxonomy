namespace BirdTaxonomy.API.Application.DTOs.Taxonomia;
public sealed class SpeciesInfoDto
{
    public int TaxonId { get; init; }
    public string NombreComun { get; init; } = string.Empty;
    public string? Descripcion { get; init; }
    public bool Domesticada { get; init; }
    public string? EstadoConservacion { get; init; }
    public string? UbicacionGeografica { get; init; }
}