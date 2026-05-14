using BirdTaxonomy.API.Application.Contracts.Persistence;
using BirdTaxonomy.API.Application.Contracts.Services;
using BirdTaxonomy.API.Application.DTOs.Taxonomia;
using BirdTaxonomy.API.Domain;
using BirdTaxonomy.API.Domain.Entities;

namespace BirdTaxonomy.API.Application.Services;

public sealed class TaxonomiaConsultaService : ITaxonomiaConsultaService
{
    private readonly IUnitOfWork _unitOfWork;

    public TaxonomiaConsultaService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IReadOnlyCollection<RankDto>> ObtenerRangosAsync(CancellationToken cancellationToken = default)
    {
        var rangos = await _unitOfWork.Ranks.GetAllAsync(cancellationToken);

        return rangos
            .OrderBy(rank => rank.Id)
            .Select(rank => new RankDto
            {
                RangoId = rank.Id,
                NombreRango = rank.Name
            })
            .ToArray();
    }

    public async Task<IReadOnlyCollection<TaxonResumenDto>> ObtenerTaxonesAsync(CancellationToken cancellationToken = default)
    {
        var taxones = await _unitOfWork.Taxones.GetAllWithRelationsAsync(cancellationToken);

        return taxones
            .OrderBy(taxon => taxon.Id)
            .Select(MapearResumen)
            .ToArray();
    }

    public async Task<TaxonDetalleDto?> ObtenerTaxonPorIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var taxon = await _unitOfWork.Taxones.GetByIdWithRelationsAsync(id, cancellationToken);
        return taxon is null ? null : MapearDetalle(taxon);
    }

    public async Task<TaxonDetalleDto> CrearTaxonAsync(CrearTaxonRequestDto request, CancellationToken cancellationToken = default)
    {
        var nombre = ValidarNombre(request.Nombre);
        await ValidarRankAsync((int)request.RankId, cancellationToken);
        ValidarSpeciesInfo(request.SpeciesInfo);

        var duplicado = await _unitOfWork.Taxones.ExistsByNameAndRankAsync(nombre, (int) request.RankId, cancellationToken: cancellationToken);
        if (duplicado)
        {
            throw new InvalidOperationException("Ya existe un taxon con el mismo nombre para ese rango.");
        }

        var taxon = new Taxon
        {
            Id = await _unitOfWork.Taxones.GetNextIdAsync(cancellationToken),
            Name = nombre,
            RankId = request.RankId,
            // Dato historico: la tabla taxon no usa identidad; el ID se controla desde la API sobre el esquema legado.
            SpeciesInfo = request.SpeciesInfo is null ? null : CrearSpeciesInfo(request.SpeciesInfo)
        };

        if (taxon.SpeciesInfo is not null)
        {
            taxon.SpeciesInfo.TaxonId = taxon.Id;
        }

        await _unitOfWork.Taxones.AddAsync(taxon, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var creado = await _unitOfWork.Taxones.GetByIdWithRelationsAsync(taxon.Id, cancellationToken);
        return MapearDetalle(creado ?? taxon);
    }

    public async Task<TaxonDetalleDto?> ActualizarTaxonAsync(int id, ActualizarTaxonRequestDto request, CancellationToken cancellationToken = default)
    {
        var taxon = await _unitOfWork.Taxones.GetByIdWithRelationsAsync(id, cancellationToken);
        if (taxon is null)
        {
            return null;
        }

        var nombre = ValidarNombre(request.Nombre);
        await ValidarRankAsync((int)request.RankId, cancellationToken);
        ValidarSpeciesInfo(request.SpeciesInfo);

        var duplicado = await _unitOfWork.Taxones.ExistsByNameAndRankAsync(nombre, (int) request.RankId, id, cancellationToken);
        if (duplicado)
        {
            throw new InvalidOperationException("Ya existe otro taxon con el mismo nombre para ese rango.");
        }

        taxon.Name = nombre;
        taxon.RankId = request.RankId;

        if (request.SpeciesInfo is null)
        {
            if (taxon.SpeciesInfo is not null)
            {
                taxon.SpeciesInfo = null;
            }
        }
        else if (taxon.SpeciesInfo is null)
        {
            taxon.SpeciesInfo = new SpeciesInfo
            {
                TaxonId = taxon.Id,
                CommonName = request.SpeciesInfo.NombreComun.Trim(),
                Description = LimpiarTextoOpcional(request.SpeciesInfo.Descripcion),
                Domesticated = request.SpeciesInfo.Domesticada,
                ConservationStatus = ConservationStatusNormalizer.Normalize(request.SpeciesInfo.EstadoConservacion),
                GeographicLocation = LimpiarTextoOpcional(request.SpeciesInfo.UbicacionGeografica)
            };
        }
        else
        {
            taxon.SpeciesInfo.CommonName = request.SpeciesInfo.NombreComun.Trim();
            taxon.SpeciesInfo.Description = LimpiarTextoOpcional(request.SpeciesInfo.Descripcion);
            taxon.SpeciesInfo.Domesticated = request.SpeciesInfo.Domesticada;
            taxon.SpeciesInfo.ConservationStatus = ConservationStatusNormalizer.Normalize(request.SpeciesInfo.EstadoConservacion);
            taxon.SpeciesInfo.GeographicLocation = LimpiarTextoOpcional(request.SpeciesInfo.UbicacionGeografica);
        }

        _unitOfWork.Taxones.Update(taxon);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var actualizado = await _unitOfWork.Taxones.GetByIdWithRelationsAsync(id, cancellationToken);
        return actualizado is null ? null : MapearDetalle(actualizado);
    }

    public async Task<bool> EliminarTaxonAsync(int id, CancellationToken cancellationToken = default)
    {
        var taxon = await _unitOfWork.Taxones.GetByIdWithRelationsAsync(id, cancellationToken);
        if (taxon is null)
        {
            return false;
        }

        _unitOfWork.Taxones.Remove(taxon);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }

    private static TaxonResumenDto MapearResumen(Taxon taxon)
    {
        return new TaxonResumenDto
        {
            TaxonId = taxon.Id,
            NombreCientifico = taxon.Name,
            RangoId = taxon.RankId ?? 0,
            NombreRango = taxon.Rank?.Name,
            SpeciesInfo = taxon.SpeciesInfo is null ? null : new SpeciesInfoDto
            {
                TaxonId = taxon.SpeciesInfo.TaxonId,
                NombreComun = taxon.SpeciesInfo.CommonName,
                Descripcion = taxon.SpeciesInfo.Description,
                Domesticada = taxon.SpeciesInfo.Domesticated,
                EstadoConservacion = taxon.SpeciesInfo.ConservationStatus,
                UbicacionGeografica = taxon.SpeciesInfo.GeographicLocation
            }
        };
    }

    private static TaxonDetalleDto MapearDetalle(Taxon taxon)
    {
        return new TaxonDetalleDto
        {
            TaxonId = taxon.Id,
            NombreCientifico = taxon.Name,
            RangoId = taxon.RankId ?? 0,
            NombreRango = taxon.Rank?.Name,
            SpeciesInfo = taxon.SpeciesInfo is null ? null : new SpeciesInfoDto
            {
                TaxonId = taxon.SpeciesInfo.TaxonId,
                NombreComun = taxon.SpeciesInfo.CommonName,
                Descripcion = taxon.SpeciesInfo.Description,
                Domesticada = taxon.SpeciesInfo.Domesticated,
                EstadoConservacion = taxon.SpeciesInfo.ConservationStatus,
                UbicacionGeografica = taxon.SpeciesInfo.GeographicLocation
            }
        };
    }
    private async Task ValidarRankAsync(int rankId, CancellationToken cancellationToken)
    {
        var rank = await _unitOfWork.Ranks.GetByIdAsync(rankId, cancellationToken);
        if (rank is null)
        {
            throw new ArgumentException("El rankId no existe.", nameof(rankId));
        }
    }

    private static string ValidarNombre(string nombre)
    {
        var limpio = nombre.Trim();
        if (string.IsNullOrWhiteSpace(limpio))
        {
            throw new ArgumentException("El nombre es obligatorio.", nameof(nombre));
        }

        if (limpio.Length > 150)
        {
            throw new ArgumentException("El nombre no puede superar 150 caracteres.", nameof(nombre));
        }

        return limpio;
    }

    private static void ValidarSpeciesInfo(CrearSpeciesInfoRequestDto? speciesInfo)
    {
        if (speciesInfo is null)
        {
            return;
        }

        ValidarNombreComun(speciesInfo.NombreComun);
        ValidarCamposOpcionales(speciesInfo.Descripcion, speciesInfo.EstadoConservacion, speciesInfo.UbicacionGeografica);
    }

    private static void ValidarSpeciesInfo(ActualizarSpeciesInfoRequestDto? speciesInfo)
    {
        if (speciesInfo is null)
        {
            return;
        }

        ValidarNombreComun(speciesInfo.NombreComun);
        ValidarCamposOpcionales(speciesInfo.Descripcion, speciesInfo.EstadoConservacion, speciesInfo.UbicacionGeografica);
    }

    private static void ValidarNombreComun(string nombreComun)
    {
        if (string.IsNullOrWhiteSpace(nombreComun))
        {
            throw new ArgumentException("El nombre comun es obligatorio.", nameof(nombreComun));
        }

        if (nombreComun.Trim().Length > 100)
        {
            throw new ArgumentException("El nombre comun no puede superar 100 caracteres.", nameof(nombreComun));
        }
    }

    private static void ValidarCamposOpcionales(string? descripcion, string? estadoConservacion, string? ubicacionGeografica)
    {
        if (!string.IsNullOrWhiteSpace(descripcion) && descripcion.Trim().Length > 200)
        {
            throw new ArgumentException("La descripcion no puede superar 200 caracteres.", nameof(descripcion));
        }

        if (!string.IsNullOrWhiteSpace(estadoConservacion))
        {
            var estado = ConservationStatusNormalizer.Normalize(estadoConservacion);
            if (estado is null)
            {
                throw new ArgumentException("El estado de conservacion no es valido.", nameof(estadoConservacion));
            }

            if (estado.Length > 50)
            {
                throw new ArgumentException("El estado de conservacion no puede superar 50 caracteres.", nameof(estadoConservacion));
            }
        }

        if (!string.IsNullOrWhiteSpace(ubicacionGeografica) && ubicacionGeografica.Trim().Length > 150)
        {
            throw new ArgumentException("La ubicacion geografica no puede superar 150 caracteres.", nameof(ubicacionGeografica));
        }
    }

    private static SpeciesInfo CrearSpeciesInfo(CrearSpeciesInfoRequestDto request)
    {
        return new SpeciesInfo
        {
            CommonName = request.NombreComun.Trim(),
            Description = LimpiarTextoOpcional(request.Descripcion),
            Domesticated = request.Domesticada,
            ConservationStatus = ConservationStatusNormalizer.Normalize(request.EstadoConservacion),
            GeographicLocation = LimpiarTextoOpcional(request.UbicacionGeografica)
        };
    }

    private static string? LimpiarTextoOpcional(string? valor)
    {
        return string.IsNullOrWhiteSpace(valor) ? null : valor.Trim();
    }
}
