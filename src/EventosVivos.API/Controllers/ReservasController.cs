using EventosVivos.Application.DTOs;
using EventosVivos.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventosVivos.API.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class ReservasController : ControllerBase
{
    private readonly IReservaService _reservaService;

    public ReservasController(IReservaService reservaService)
    {
        _reservaService = reservaService;
    }

    /// <summary>
    /// Crea una nueva reserva de entradas (RF-03).
    /// </summary>
    [HttpPost]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ReservaResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Crear([FromBody] CrearReservaRequest request)
    {
        var result = await _reservaService.CrearReservaAsync(request);
        return CreatedAtAction(nameof(Crear), new { id = result.Id }, result);
    }

    /// <summary>
    /// Confirma el pago de una reserva (RF-04). Requiere autenticacion.
    /// </summary>
    [HttpPut("{reservaId}/confirmar")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ReservaResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ConfirmarPago(int reservaId)
    {
        var result = await _reservaService.ConfirmarPagoAsync(reservaId);
        return Ok(result);
    }

    /// <summary>
    /// Cancela una reserva confirmada (RF-05).
    /// </summary>
    [HttpPut("{reservaId}/cancelar")]
    [Authorize]
    [ProducesResponseType(typeof(ReservaResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Cancelar(int reservaId)
    {
        var result = await _reservaService.CancelarReservaAsync(reservaId);
        return Ok(result);
    }
}

