using EventosVivos.Domain.Entities;
using EventosVivos.Domain.Enums;

namespace EventosVivos.Domain.Interfaces;

public interface IEventoRepository
{
    Task<Evento?> GetByIdAsync(int id);
    Task<Evento?> GetByIdWithReservasAsync(int id);
    Task<IEnumerable<Evento>> GetAllAsync(TipoEvento? tipo, DateTime? fechaDesde, DateTime? fechaHasta, int? venueId, EstadoEvento? estado, string? titulo);
    Task<Evento> CreateAsync(Evento evento);
    Task UpdateAsync(Evento evento);
    Task<bool> ExisteSuperposicionAsync(int venueId, DateTime inicio, DateTime fin, int? eventoIdExcluir = null);
}
