using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using EventosVivos.Application.DTOs;
using EventosVivos.Application.Interfaces;
using EventosVivos.Domain.Entities;
using EventosVivos.Domain.Exceptions;
using EventosVivos.Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace EventosVivos.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUsuarioRepository _usuarioRepo;
    private readonly IConfiguration _configuration;

    public AuthService(IUsuarioRepository usuarioRepo, IConfiguration configuration)
    {
        _usuarioRepo = usuarioRepo;
        _configuration = configuration;
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        var usuario = await _usuarioRepo.GetByNombreUsuarioAsync(request.NombreUsuario)
            ?? throw new BusinessRuleException("Credenciales invalidas.");

        if (!BCrypt.Net.BCrypt.Verify(request.Password, usuario.PasswordHash))
            throw new BusinessRuleException("Credenciales invalidas.");

        var token = GenerarToken(usuario);
        return new AuthResponse(token, usuario.NombreUsuario, usuario.Rol);
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        var existente = await _usuarioRepo.GetByNombreUsuarioAsync(request.NombreUsuario);
        if (existente != null)
            throw new BusinessRuleException("El nombre de usuario ya esta registrado.");

        var usuario = new Usuario
        {
            NombreUsuario = request.NombreUsuario,
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Rol = "Admin"
        };

        var created = await _usuarioRepo.CreateAsync(usuario);
        var token = GenerarToken(created);
        return new AuthResponse(token, created.NombreUsuario, created.Rol);
    }

    private string GenerarToken(Usuario usuario)
    {
        var jwtSettings = _configuration.GetSection("JwtSettings");
        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]!));

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
            new Claim(ClaimTypes.Name, usuario.NombreUsuario),
            new Claim(ClaimTypes.Email, usuario.Email),
            new Claim(ClaimTypes.Role, usuario.Rol)
        };

        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            issuer: jwtSettings["Issuer"],
            audience: jwtSettings["Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(2),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}

