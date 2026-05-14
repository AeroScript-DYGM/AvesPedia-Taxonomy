namespace BirdTaxonomy.API.Application.DTOs.Taxonomia;
public sealed class TaxonDetalleDto
{
    public int TaxonId { get; init; }
    public string? NombreCientifico { get; init; }
    public int? RangoId { get; init; }
    public string? NombreRango { get; init; }
    public SpeciesInfoDto? SpeciesInfo { get; init; }
}
