using System.ComponentModel.DataAnnotations;
using EventosVivos.Domain.Enums;

namespace EventosVivos.Application.DTOs;

public record CrearEventoRequest(
    [Required, StringLength(100, MinimumLength = 5)] string Titulo,
    [Required, StringLength(500, MinimumLength = 10)] string Descripcion,
    [Required, Range(1, int.MaxValue)] int VenueId,
    [Required, Range(1, int.MaxValue)] int CapacidadMaxima,
    [Required] DateTime FechaInicio,
    [Required] DateTime FechaFin,
    [Required, Range(0.01, double.MaxValue)] decimal PrecioEntrada,
    [Required] TipoEvento TipoEvento
);

public record EventoResponse(
    int Id,
    string Titulo,
    string Descripcion,
    int VenueId,
    string VenueNombre,
    int CapacidadMaxima,
    DateTime FechaInicio,
    DateTime FechaFin,
    decimal PrecioEntrada,
    string TipoEvento,
    string Estado
);

public record CrearReservaRequest(
    [Required, Range(1, int.MaxValue)] int EventoId,
    [Required, Range(1, int.MaxValue)] int Cantidad,
    [Required, StringLength(200, MinimumLength = 2)] string NombreComprador,
    [Required, EmailAddress, StringLength(300)] string EmailComprador
);

public record ReservaResponse(
    int Id,
    int EventoId,
    string EventoTitulo,
    int Cantidad,
    string NombreComprador,
    string EmailComprador,
    string Estado,
    string? CodigoReserva,
    DateTime FechaCreacion,
    DateTime? FechaCancelacion,
    bool EntradasPerdidas
);

public record ReporteOcupacionResponse(
    int EventoId,
    string Titulo,
    int EntradasVendidas,
    int EntradasDisponibles,
    decimal PorcentajeOcupacion,
    decimal TotalIngresos,
    string Estado
);

public record LoginRequest(
    [Required] string NombreUsuario,
    [Required] string Password
);

public record RegisterRequest(
    [Required, StringLength(50, MinimumLength = 3)] string NombreUsuario,
    [Required, EmailAddress] string Email,
    [Required, StringLength(100, MinimumLength = 6)] string Password
);

public record AuthResponse(
    string Token,
    string NombreUsuario,
    string Rol
);

public record EventoFilterRequest(
    TipoEvento? TipoEvento,
    DateTime? FechaDesde,
    DateTime? FechaHasta,
    int? VenueId,
    EstadoEvento? Estado,
    string? Titulo
);
