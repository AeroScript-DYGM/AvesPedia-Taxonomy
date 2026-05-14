using BirdTaxonomy.API.Application.Contracts.Persistence;
using BirdTaxonomy.API.Application.Contracts.Repositories;

using BirdTaxonomy.API.Persistence;

namespace BirdTaxonomy.API.Infrastructure.Persistence;

public sealed class UnitOfWork : IUnitOfWork
{
    private readonly BirdTaxonomyDbContext _context;

    public UnitOfWork(BirdTaxonomyDbContext context, IRankRepository rankRepository, ITaxonRepository taxonRepository)
    {
        _context = context;
        Ranks = rankRepository;
        Taxones = taxonRepository;
    }

    public IRankRepository Ranks { get; }
    public ITaxonRepository Taxones { get; }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _context.SaveChangesAsync(cancellationToken);
    }
}
