using BirdTaxonomy.API.Application.Contracts.Repositories;
using BirdTaxonomy.API.Domain.Entities;
using BirdTaxonomy.API.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BirdTaxonomy.API.Infrastructure.Repositories;

public sealed class TaxonRepository : GenericRepository<Taxon>, ITaxonRepository
{
    public TaxonRepository(BirdTaxonomyDbContext context)
        : base(context)
    {
    }

    public async Task<IReadOnlyCollection<Taxon>> GetAllWithRelationsAsync(CancellationToken cancellationToken = default)
    {
        return await Context.Taxones
            .AsNoTracking()
            .Include(taxon => taxon.Rank)
            .Include(taxon => taxon.SpeciesInfo)
            .ToListAsync(cancellationToken);
    }

    public async Task<Taxon?> GetByIdWithRelationsAsync(int id, CancellationToken cancellationToken = default)
    {
        return await Context.Taxones
            .Include(taxon => taxon.Rank)
            .Include(taxon => taxon.SpeciesInfo)
            .FirstOrDefaultAsync(taxon => taxon.Id == id, cancellationToken);
    }

    public async Task<int> GetNextIdAsync(CancellationToken cancellationToken = default)
    {
        var maxId = await Context.Taxones
            .AsNoTracking()
            .Select(taxon => (int?)taxon.Id)
            .MaxAsync(cancellationToken);

        return (maxId ?? 0) + 1;
    }

    public async Task<bool> ExistsByNameAndRankAsync(
        string nombre,
        int rankId,
        int? excludeId = null,
        CancellationToken cancellationToken = default)
    {
        return await Context.Taxones
            .AsNoTracking()
            .AnyAsync(
                taxon => taxon.Name == nombre
                    && taxon.RankId == rankId
                    && (!excludeId.HasValue || taxon.Id != excludeId.Value),
                cancellationToken);
    }
}
