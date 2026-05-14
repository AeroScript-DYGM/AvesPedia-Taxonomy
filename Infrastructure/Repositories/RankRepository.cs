using BirdTaxonomy.API.Application.Contracts.Repositories;
using BirdTaxonomy.API.Domain.Entities;
using BirdTaxonomy.API.Persistence;

namespace BirdTaxonomy.API.Infrastructure.Repositories;

public sealed class RankRepository : GenericRepository<Rank>, IRankRepository
{
    public RankRepository(BirdTaxonomyDbContext context)
        : base(context)
    {
    }
}
