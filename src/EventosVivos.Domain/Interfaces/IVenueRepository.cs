using EventosVivos.Domain.Entities;

namespace EventosVivos.Domain.Interfaces;

public interface IVenueRepository
{
    Task<Venue?> GetByIdAsync(int id);
    Task<IEnumerable<Venue>> GetAllAsync();
}
