namespace BirdTaxonomy.API.Application.DTOs.Taxonomia;

public sealed class CrearTaxonRequestDto
{
    public string Nombre { get; init; } = string.Empty;
    public int? RankId { get; init; }
    public CrearSpeciesInfoRequestDto? SpeciesInfo { get; init; }
}
