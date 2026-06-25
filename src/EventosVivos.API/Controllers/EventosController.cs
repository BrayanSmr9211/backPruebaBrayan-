using EventosVivos.Application.DTOs;
using EventosVivos.Application.Interfaces;
using EventosVivos.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventosVivos.API.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class EventosController : ControllerBase
{
    private readonly IEventoService _eventoService;

    public EventosController(IEventoService eventoService)
    {
        _eventoService = eventoService;
    }

    /// <summary>
    /// Crea un nuevo evento (RF-01).
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(EventoResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Crear([FromBody] CrearEventoRequest request)
    {
        var result = await _eventoService.CrearEventoAsync(request);
        return CreatedAtAction(nameof(ObtenerReporte), new { eventoId = result.Id }, result);
    }

    /// <summary>
    /// Lista eventos con filtros opcionales (RF-02).
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(IEnumerable<EventoResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Listar(
        [FromQuery] TipoEvento? tipoEvento,
        [FromQuery] DateTime? fechaDesde,
        [FromQuery] DateTime? fechaHasta,
        [FromQuery] int? venueId,
        [FromQuery] EstadoEvento? estado,
        [FromQuery] string? titulo)
    {
        var filter = new EventoFilterRequest(tipoEvento, fechaDesde, fechaHasta, venueId, estado, titulo);
        var result = await _eventoService.ListarEventosAsync(filter);
        return Ok(result);
    }

    /// <summary>
    /// Obtiene el reporte de ocupacion de un evento (RF-06).
    /// </summary>
    [HttpGet("{eventoId}/reporte")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ReporteOcupacionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObtenerReporte(int eventoId)
    {
        var result = await _eventoService.ObtenerReporteOcupacionAsync(eventoId);
        return Ok(result);
    }
}

