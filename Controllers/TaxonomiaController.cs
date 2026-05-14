using BirdTaxonomy.API.Application.Contracts.Services;
using BirdTaxonomy.API.Application.DTOs.Taxonomia;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;

namespace BirdTaxonomy.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class TaxonomiaController : ControllerBase
{
    private readonly ITaxonomiaConsultaService _taxonomiaConsultaService;

    public TaxonomiaController(ITaxonomiaConsultaService taxonomiaConsultaService)
    {
        _taxonomiaConsultaService = taxonomiaConsultaService;
    }

    [HttpGet("rangos")]
    [ProducesResponseType(typeof(IReadOnlyCollection<RankDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IReadOnlyCollection<RankDto>>> GetRangosAsync(CancellationToken cancellationToken)
    {
        var response = await _taxonomiaConsultaService.ObtenerRangosAsync(cancellationToken);
        return Ok(response);
    }

    [HttpGet("taxones")]
    [ProducesResponseType(typeof(IReadOnlyCollection<TaxonResumenDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IReadOnlyCollection<TaxonResumenDto>>> GetTaxonesAsync(CancellationToken cancellationToken)
    {
        var response = await _taxonomiaConsultaService.ObtenerTaxonesAsync(cancellationToken);
        return Ok(response);
    }

    [HttpGet("taxones/{id:int}")]
    [ProducesResponseType(typeof(TaxonDetalleDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<TaxonDetalleDto>> GetTaxonByIdAsync(int id, CancellationToken cancellationToken)
    {
        var response = await _taxonomiaConsultaService.ObtenerTaxonPorIdAsync(id, cancellationToken);
        if (response is null)
        {
            return NotFound();
        }

        return Ok(response);
    }

    [HttpPost("taxones")]
    [ProducesResponseType(typeof(TaxonDetalleDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<TaxonDetalleDto>> CreateTaxonAsync(
        [FromBody] CrearTaxonRequestDto request,
        CancellationToken cancellationToken)
    {
        try
        {
            var response = await _taxonomiaConsultaService.CrearTaxonAsync(request, cancellationToken);
            return CreatedAtAction(nameof(GetTaxonByIdAsync), new { id = response.TaxonId }, response);
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
        catch (InvalidOperationException exception)
        {
            return Conflict(new { message = exception.Message });
        }
        catch (DbUpdateException exception)
        {
            return MapearDbUpdateException(exception);
        }
    }

    [HttpPut("taxones/{id:int}")]
    [ProducesResponseType(typeof(TaxonDetalleDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<TaxonDetalleDto>> UpdateTaxonAsync(
        int id,
        [FromBody] ActualizarTaxonRequestDto request,
        CancellationToken cancellationToken)
    {
        try
        {
            var response = await _taxonomiaConsultaService.ActualizarTaxonAsync(id, request, cancellationToken);
            if (response is null)
            {
                return NotFound();
            }

            return Ok(response);
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
        catch (InvalidOperationException exception)
        {
            return Conflict(new { message = exception.Message });
        }
        catch (DbUpdateException exception)
        {
            return MapearDbUpdateException(exception);
        }
    }

    [HttpDelete("taxones/{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteTaxonAsync(int id, CancellationToken cancellationToken)
    {
        try
        {
            var eliminado = await _taxonomiaConsultaService.EliminarTaxonAsync(id, cancellationToken);
            if (!eliminado)
            {
                return NotFound();
            }

            return NoContent();
        }
        catch (DbUpdateException exception)
        {
            return MapearDbUpdateException(exception);
        }
    }

    private ObjectResult MapearDbUpdateException(DbUpdateException exception)
    {
        var detalle = exception.InnerException?.Message ?? exception.Message;

        if (detalle.Contains("FOREIGN KEY", StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest(new { message = "La operacion viola una referencia valida de la base de datos.", detail = detalle });
        }

        if (detalle.Contains("UNIQUE", StringComparison.OrdinalIgnoreCase)
            || detalle.Contains("duplicate", StringComparison.OrdinalIgnoreCase))
        {
            return Conflict(new { message = "La operacion genera un duplicado no permitido por la base de datos.", detail = detalle });
        }

        return StatusCode(StatusCodes.Status500InternalServerError, new
        {
            message = "Ocurrio una resistencia de persistencia al guardar cambios en la base de datos.",
            detail = detalle
        });
    }
}
