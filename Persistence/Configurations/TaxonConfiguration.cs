using BirdTaxonomy.API.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BirdTaxonomy.API.Persistence.Configurations;

public class TaxonConfiguration : IEntityTypeConfiguration<Taxon>
{
    public void Configure(EntityTypeBuilder<Taxon> builder)
    {
        builder.ToTable("taxon");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("ID").ValueGeneratedNever();
        builder.Property(x => x.Name).HasColumnName("name_").IsRequired().IsUnicode(false);
        builder.Property(x => x.RankId).HasColumnName("rankid").IsRequired();

        builder.HasOne(x => x.Rank)
            .WithMany(r => r.Taxones)
            .HasForeignKey(x => x.RankId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.SpeciesInfo)
            .WithOne(x => x.Taxon)
            .HasForeignKey<SpeciesInfo>(x => x.TaxonId)
            // Dato historico: el esquema legado no fue creado desde este ORM; se mantiene el comentario aunque el mapeo se alinea al requerimiento actual.
            .OnDelete(DeleteBehavior.Cascade);
    }
}
