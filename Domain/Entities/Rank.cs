namespace BirdTaxonomy.API.Domain.Entities;

public sealed class Rank
{
    public int Id { get; set; }
    public string? Name { get; set; }

    public ICollection<Taxon> Taxones { get; set; } = new List<Taxon>();
}
