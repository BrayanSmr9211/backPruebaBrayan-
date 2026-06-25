using EventosVivos.Domain.Entities;
using EventosVivos.Domain.Enums;
using EventosVivos.Domain.Interfaces;
using EventosVivos.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EventosVivos.Infrastructure.Repositories;

public class EventoRepository : IEventoRepository
{
    private readonly AppDbContext _context;

    public EventoRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Evento?> GetByIdAsync(int id)
    {
        return await _context.Eventos
            .Include(e => e.Venue)
            .FirstOrDefaultAsync(e => e.Id == id);
    }

    public async Task<Evento?> GetByIdWithReservasAsync(int id)
    {
        return await _context.Eventos
            .Include(e => e.Venue)
            .Include(e => e.Reservas)
            .FirstOrDefaultAsync(e => e.Id == id);
    }

    public async Task<IEnumerable<Evento>> GetAllAsync(
        TipoEvento? tipo, DateTime? fechaDesde, DateTime? fechaHasta,
        int? venueId, EstadoEvento? estado, string? titulo)
    {
        var query = _context.Eventos
            .Include(e => e.Venue)
            .Include(e => e.Reservas)
            .AsQueryable();

        if (tipo.HasValue)
            query = query.Where(e => e.TipoEvento == tipo.Value);

        if (fechaDesde.HasValue)
            query = query.Where(e => e.FechaInicio >= fechaDesde.Value);

        if (fechaHasta.HasValue)
            query = query.Where(e => e.FechaInicio <= fechaHasta.Value);

        if (venueId.HasValue)
            query = query.Where(e => e.VenueId == venueId.Value);

        if (estado.HasValue)
            query = query.Where(e => e.Estado == estado.Value);

        if (!string.IsNullOrWhiteSpace(titulo))
            query = query.Where(e => e.Titulo.ToLower().Contains(titulo.ToLower()));

        return await query.OrderBy(e => e.FechaInicio).ToListAsync();
    }

    public async Task<Evento> CreateAsync(Evento evento)
    {
        _context.Eventos.Add(evento);
        await _context.SaveChangesAsync();
        return evento;
    }

    public async Task UpdateAsync(Evento evento)
    {
        _context.Eventos.Update(evento);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> ExisteSuperposicionAsync(int venueId, DateTime inicio, DateTime fin, int? eventoIdExcluir = null)
    {
        var query = _context.Eventos
            .Where(e => e.VenueId == venueId && e.Estado == EstadoEvento.Activo)
            .Where(e => e.FechaInicio < fin && e.FechaFin > inicio);

        if (eventoIdExcluir.HasValue)
            query = query.Where(e => e.Id != eventoIdExcluir.Value);

        return await query.AnyAsync();
    }
}

