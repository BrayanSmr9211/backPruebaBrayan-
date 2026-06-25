using EventosVivos.Domain.Entities;
using EventosVivos.Domain.Interfaces;
using EventosVivos.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EventosVivos.Infrastructure.Repositories;

public class UsuarioRepository : IUsuarioRepository
{
    private readonly AppDbContext _context;

    public UsuarioRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Usuario?> GetByNombreUsuarioAsync(string nombreUsuario)
    {
        return await _context.Usuarios
            .FirstOrDefaultAsync(u => u.NombreUsuario == nombreUsuario);
    }

    public async Task<Usuario> CreateAsync(Usuario usuario)
    {
        _context.Usuarios.Add(usuario);
        await _context.SaveChangesAsync();
        return usuario;
    }
}

