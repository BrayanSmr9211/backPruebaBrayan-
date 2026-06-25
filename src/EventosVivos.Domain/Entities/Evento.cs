using EventosVivos.Domain.Enums;

namespace EventosVivos.Domain.Entities;

public class Evento
{
    public int Id { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public int VenueId { get; set; }
    public Venue Venue { get; set; } = null!;
    public int CapacidadMaxima { get; set; }
    public DateTime FechaInicio { get; set; }
    public DateTime FechaFin { get; set; }
    public decimal PrecioEntrada { get; set; }
    public TipoEvento TipoEvento { get; set; }
    public EstadoEvento Estado { get; set; } = EstadoEvento.Activo;
    public ICollection<Reserva> Reservas { get; set; } = new List<Reserva>();

    public int EntradasDisponibles()
    {
        var confirmadas = Reservas
            .Where(r => r.Estado == EstadoReserva.Confirmada)
            .Sum(r => r.Cantidad);
        var pendientes = Reservas
            .Where(r => r.Estado == EstadoReserva.PendientePago)
            .Sum(r => r.Cantidad);
        var perdidas = Reservas
            .Where(r => r.Estado == EstadoReserva.Cancelada && r.EntradasPerdidas)
            .Sum(r => r.Cantidad);
        return CapacidadMaxima - confirmadas - pendientes - perdidas;
    }

    public int EntradasVendidas()
    {
        return Reservas
            .Where(r => r.Estado == EstadoReserva.Confirmada)
            .Sum(r => r.Cantidad);
    }

    public int EntradasPerdidas()
    {
        return Reservas
            .Where(r => r.Estado == EstadoReserva.Cancelada && r.EntradasPerdidas)
            .Sum(r => r.Cantidad);
    }
}
