using BirdTaxonomy.API.Application.DTOs.Taxonomia;

namespace BirdTaxonomy.API.Application.Contracts.Services;

public interface ITaxonomiaConsultaService
{
    Task<IReadOnlyCollection<RankDto>> ObtenerRangosAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<TaxonResumenDto>> ObtenerTaxonesAsync(CancellationToken cancellationToken = default);
    Task<TaxonDetalleDto?> ObtenerTaxonPorIdAsync(int id, CancellationToken cancellationToken = default);
    Task<TaxonDetalleDto> CrearTaxonAsync(CrearTaxonRequestDto request, CancellationToken cancellationToken = default);
    Task<TaxonDetalleDto?> ActualizarTaxonAsync(int id, ActualizarTaxonRequestDto request, CancellationToken cancellationToken = default);
    Task<bool> EliminarTaxonAsync(int id, CancellationToken cancellationToken = default);
}
