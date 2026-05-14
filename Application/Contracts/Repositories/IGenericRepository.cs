namespace BirdTaxonomy.API.Application.Contracts.Repositories;

public interface IGenericRepository<TEntity>
    where TEntity : class
{

    //supongo que esto es para que no se tenga que crear un repositorio para cada entidad, sino que se pueda usar este genérico para todas las entidades, y asi evitar la repetición de código, pero no se si esto es lo mejor o si es mejor tener un repositorio específico para cada entidad, aunque eso si puede generar mucho código repetido.? si tiene uso ponlo, sino se ve profesional ysolo redundad quitalo,a mi me parece la segunda.
    Task<IReadOnlyCollection<TEntity>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<TEntity?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task AddAsync(TEntity entity, CancellationToken cancellationToken = default);
    void Update(TEntity entity);
    void Remove(TEntity entity);
}
