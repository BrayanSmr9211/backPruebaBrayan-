using EventosVivos.Domain.Entities;

namespace EventosVivos.Domain.Interfaces;

public interface IUsuarioRepository
{
    Task<Usuario?> GetByNombreUsuarioAsync(string nombreUsuario);
    Task<Usuario> CreateAsync(Usuario usuario);
}
