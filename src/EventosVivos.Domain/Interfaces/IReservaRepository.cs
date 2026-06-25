using EventosVivos.Domain.Entities;

namespace EventosVivos.Domain.Interfaces;

public interface IReservaRepository
{
    Task<Reserva?> GetByIdAsync(int id);
    Task<Reserva> CreateAsync(Reserva reserva);
    Task UpdateAsync(Reserva reserva);
}
