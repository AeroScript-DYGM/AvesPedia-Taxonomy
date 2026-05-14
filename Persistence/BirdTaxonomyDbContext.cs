using BirdTaxonomy.API.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BirdTaxonomy.API.Persistence;

public sealed class BirdTaxonomyDbContext : DbContext
{
    public BirdTaxonomyDbContext(DbContextOptions<BirdTaxonomyDbContext> options)
        : base(options)
    {
    }


    // los DbSet representan las tablas de la base de datos, cada una corresponde a una entidad del dominio, O TIENEN PROBLEMAS DE RELACION CON LA DE BASE DE DATOS SINO QUE LOS NOMBRES DE CADA UNO SEA EN MAYUSCULA Y TENGA EL MISMO NOMBRE EN BASE DE DATOS?
    public DbSet<Rank> Ranks => Set<Rank>();
    public DbSet<Taxon> Taxones => Set<Taxon>();
    public DbSet<SpeciesInfo> SpeciesInfos => Set<SpeciesInfo>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(BirdTaxonomyDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
