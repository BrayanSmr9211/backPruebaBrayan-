using EventosVivos.Application.DTOs;

namespace EventosVivos.Application.Interfaces;

public interface IReservaService
{
    Task<ReservaResponse> CrearReservaAsync(CrearReservaRequest request);
    Task<ReservaResponse> ConfirmarPagoAsync(int reservaId);
    Task<ReservaResponse> CancelarReservaAsync(int reservaId);
}
