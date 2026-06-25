using EventosVivos.Domain.Entities;
using EventosVivos.Domain.Interfaces;
using EventosVivos.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EventosVivos.Infrastructure.Repositories;

public class ReservaRepository : IReservaRepository
{
    private readonly AppDbContext _context;

    public ReservaRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Reserva?> GetByIdAsync(int id)
    {
        return await _context.Reservas
            .Include(r => r.Evento)
            .FirstOrDefaultAsync(r => r.Id == id);
    }

    public async Task<Reserva> CreateAsync(Reserva reserva)
    {
        _context.Reservas.Add(reserva);
        await _context.SaveChangesAsync();
        return reserva;
    }

    public async Task UpdateAsync(Reserva reserva)
    {
        _context.Reservas.Update(reserva);
        await _context.SaveChangesAsync();
    }
}

