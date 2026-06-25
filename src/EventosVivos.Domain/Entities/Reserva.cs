using EventosVivos.Domain.Enums;

namespace EventosVivos.Domain.Entities;

public class Reserva
{
    public int Id { get; set; }
    public int EventoId { get; set; }
    public Evento Evento { get; set; } = null!;
    public int Cantidad { get; set; }
    public string NombreComprador { get; set; } = string.Empty;
    public string EmailComprador { get; set; } = string.Empty;
    public EstadoReserva Estado { get; set; } = EstadoReserva.PendientePago;
    public string? CodigoReserva { get; set; }
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
    public DateTime? FechaCancelacion { get; set; }
    public bool EntradasPerdidas { get; set; } = false;
}
