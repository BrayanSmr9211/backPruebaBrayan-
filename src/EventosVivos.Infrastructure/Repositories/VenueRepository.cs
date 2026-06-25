using EventosVivos.Domain.Entities;
using EventosVivos.Domain.Interfaces;
using EventosVivos.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EventosVivos.Infrastructure.Repositories;

public class VenueRepository : IVenueRepository
{
    private readonly AppDbContext _context;

    public VenueRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Venue?> GetByIdAsync(int id)
    {
        return await _context.Venues.FindAsync(id);
    }

    public async Task<IEnumerable<Venue>> GetAllAsync()
    {
        return await _context.Venues.ToListAsync();
    }
}

