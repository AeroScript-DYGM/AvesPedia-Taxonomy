namespace BirdTaxonomy.API.Domain.Entities;

public sealed class Taxon
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public int? RankId { get; set; }

    public Rank? Rank { get; set; }
    public SpeciesInfo? SpeciesInfo { get; set; }
}
