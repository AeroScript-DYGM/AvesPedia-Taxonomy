namespace BirdTaxonomy.API.Application.DTOs.Taxonomia;

public sealed class ActualizarTaxonRequestDto
{
    public string Nombre { get; init; } = string.Empty;
    public int? RankId { get; init; }
    public ActualizarSpeciesInfoRequestDto? SpeciesInfo { get; init; }
}
