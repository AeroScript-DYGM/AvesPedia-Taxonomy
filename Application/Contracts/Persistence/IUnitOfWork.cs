using BirdTaxonomy.API.Application.Contracts.Repositories;

namespace BirdTaxonomy.API.Application.Contracts.Persistence;

public interface IUnitOfWork
{

    // PORQUE NO TIENE SPECIES_INFOS O INFO para mi esto esta mal.
    IRankRepository Ranks { get; }
    ITaxonRepository Taxones { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
