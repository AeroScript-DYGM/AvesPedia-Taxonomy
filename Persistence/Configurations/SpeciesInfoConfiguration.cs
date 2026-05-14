using BirdTaxonomy.API.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BirdTaxonomy.API.Persistence.Configurations;

public class SpeciesInfoConfiguration : IEntityTypeConfiguration<SpeciesInfo>
{
    public void Configure(EntityTypeBuilder<SpeciesInfo> builder)
    {
        builder.ToTable("species_info");
        builder.HasKey(x => x.TaxonId);
        builder.Property(x => x.TaxonId).HasColumnName("taxonid");
        builder.Property(x => x.CommonName).HasColumnName("commun_name").IsUnicode(false);
        builder.Property(x => x.ConservationStatus).HasColumnName("conservation_status").IsUnicode(false);
        builder.Property(x => x.Description).HasColumnName("description_").IsUnicode(false);
        builder.Property(x => x.Domesticated).HasColumnName("domesticated");
        builder.Property(x => x.GeographicLocation).HasColumnName("geographic_location").IsUnicode(false);
    }
}
