namespace BirdTaxonomy.API.Domain.Entities;

public sealed class SpeciesInfo
{
    public int TaxonId { get; set; }
    public string CommonName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool Domesticated { get; set; }
    public string? ConservationStatus { get; set; }
    public string? GeographicLocation { get; set; }

    public Taxon? Taxon { get; set; }
}
