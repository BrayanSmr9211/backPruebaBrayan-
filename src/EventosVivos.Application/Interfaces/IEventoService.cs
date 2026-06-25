using EventosVivos.Application.DTOs;

namespace EventosVivos.Application.Interfaces;

public interface IEventoService
{
    Task<EventoResponse> CrearEventoAsync(CrearEventoRequest request);
    Task<IEnumerable<EventoResponse>> ListarEventosAsync(EventoFilterRequest filter);
    Task<ReporteOcupacionResponse> ObtenerReporteOcupacionAsync(int eventoId);
}
