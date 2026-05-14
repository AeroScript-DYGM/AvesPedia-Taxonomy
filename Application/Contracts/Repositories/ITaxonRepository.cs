using BirdTaxonomy.API.Domain.Entities;

namespace BirdTaxonomy.API.Application.Contracts.Repositories;
// misma pregunta que con el IGenericRepository, para que esta este, si ni sirve? si tiene uso ponlo, sino se ve profesional ysolo redundad quitalo,a mi me parece la segunda.
public interface ITaxonRepository : IGenericRepository<Taxon>
{
    Task<IReadOnlyCollection<Taxon>> GetAllWithRelationsAsync(CancellationToken cancellationToken = default);
    Task<Taxon?> GetByIdWithRelationsAsync(int id, CancellationToken cancellationToken = default);
    Task<int> GetNextIdAsync(CancellationToken cancellationToken = default);
    Task<bool> ExistsByNameAndRankAsync(string nombre, int rankId, int? excludeId = null, CancellationToken cancellationToken = default);
}
