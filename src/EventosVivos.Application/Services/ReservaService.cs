using EventosVivos.Application.DTOs;
using EventosVivos.Application.Interfaces;
using EventosVivos.Domain.Entities;
using EventosVivos.Domain.Enums;
using EventosVivos.Domain.Exceptions;
using EventosVivos.Domain.Interfaces;

namespace EventosVivos.Application.Services;

public class ReservaService : IReservaService
{
    private readonly IReservaRepository _reservaRepo;
    private readonly IEventoRepository _eventoRepo;

    public ReservaService(IReservaRepository reservaRepo, IEventoRepository eventoRepo)
    {
        _reservaRepo = reservaRepo;
        _eventoRepo = eventoRepo;
    }

    public async Task<ReservaResponse> CrearReservaAsync(CrearReservaRequest request)
    {
        var evento = await _eventoRepo.GetByIdWithReservasAsync(request.EventoId)
            ?? throw new NotFoundException($"Evento con Id {request.EventoId} no encontrado.");

        if (evento.Estado != EstadoEvento.Activo)
            throw new BusinessRuleException("Solo se pueden reservar entradas de eventos activos.");

        // RN-06: Verificar si el evento ya termino
        if (DateTime.UtcNow > evento.FechaFin)
            throw new BusinessRuleException("El evento ya ha finalizado.");

        // RN-04: Restriccion de reserva tardia (menos de 1 hora)
        if ((evento.FechaInicio - DateTime.UtcNow).TotalHours < 1)
            throw new BusinessRuleException("No se permiten reservas para eventos que inicien en menos de 1 hora.");

        // RF-03: Si faltan menos de 24h, maximo 5 entradas (prioridad sobre RN-05)
        if ((evento.FechaInicio - DateTime.UtcNow).TotalHours < 24 && request.Cantidad > 5)
            throw new BusinessRuleException("Si el evento tiene menos de 24 horas para iniciar, solo se permite reservar maximo 5 entradas por transaccion.");

        // RN-05: Eventos con precio > $100 limitan a 10 entradas (si no aplica RF-03)
        if ((evento.FechaInicio - DateTime.UtcNow).TotalHours >= 24
            && evento.PrecioEntrada > 100 && request.Cantidad > 10)
            throw new BusinessRuleException("Eventos con precio mayor a $100 limitan a maximo 10 entradas por transaccion.");

        // Validar disponibilidad
        var disponibles = evento.EntradasDisponibles();
        if (request.Cantidad > disponibles)
            throw new BusinessRuleException($"No hay suficientes entradas disponibles. Disponibles: {disponibles}.");

        var reserva = new Reserva
        {
            EventoId = request.EventoId,
            Cantidad = request.Cantidad,
            NombreComprador = request.NombreComprador,
            EmailComprador = request.EmailComprador,
            Estado = EstadoReserva.PendientePago,
            FechaCreacion = DateTime.UtcNow
        };

        var created = await _reservaRepo.CreateAsync(reserva);

        return MapToResponse(created, evento.Titulo);
    }

    public async Task<ReservaResponse> ConfirmarPagoAsync(int reservaId)
    {
        var reserva = await _reservaRepo.GetByIdAsync(reservaId)
            ?? throw new NotFoundException($"Reserva con Id {reservaId} no encontrada.");

        if (reserva.Estado == EstadoReserva.Confirmada)
            throw new BusinessRuleException("La reserva ya esta confirmada.");

        if (reserva.Estado == EstadoReserva.Cancelada)
            throw new BusinessRuleException("No se puede confirmar una reserva cancelada.");

        reserva.Estado = EstadoReserva.Confirmada;
        reserva.CodigoReserva = GenerarCodigoReserva();

        await _reservaRepo.UpdateAsync(reserva);

        return MapToResponse(reserva, reserva.Evento?.Titulo ?? string.Empty);
    }

    public async Task<ReservaResponse> CancelarReservaAsync(int reservaId)
    {
        var reserva = await _reservaRepo.GetByIdAsync(reservaId)
            ?? throw new NotFoundException($"Reserva con Id {reservaId} no encontrada.");

        if (reserva.Estado == EstadoReserva.Cancelada)
            throw new BusinessRuleException("La reserva ya esta cancelada.");

        if (reserva.Estado == EstadoReserva.PendientePago)
            throw new BusinessRuleException("No se puede cancelar una reserva en estado pendiente_pago. Solo se cancelan reservas confirmadas.");

        if (reserva.Estado != EstadoReserva.Confirmada)
            throw new BusinessRuleException("Solo se pueden cancelar reservas con estado confirmada.");

        reserva.Estado = EstadoReserva.Cancelada;
        reserva.FechaCancelacion = DateTime.UtcNow;

        // RN-07: Penalizacion si faltan menos de 48h
        var evento = await _eventoRepo.GetByIdAsync(reserva.EventoId);
        if (evento != null && (evento.FechaInicio - DateTime.UtcNow).TotalHours < 48)
        {
            reserva.EntradasPerdidas = true;
        }

        await _reservaRepo.UpdateAsync(reserva);

        return MapToResponse(reserva, evento?.Titulo ?? string.Empty);
    }

    private static string GenerarCodigoReserva()
    {
        var random = new Random();
        var digits = random.Next(100000, 999999);
        return $"EV-{digits}";
    }

    private static ReservaResponse MapToResponse(Reserva reserva, string eventoTitulo)
    {
        return new ReservaResponse(
            reserva.Id,
            reserva.EventoId,
            eventoTitulo,
            reserva.Cantidad,
            reserva.NombreComprador,
            reserva.EmailComprador,
            reserva.Estado.ToString().ToLower(),
            reserva.CodigoReserva,
            reserva.FechaCreacion,
            reserva.FechaCancelacion,
            reserva.EntradasPerdidas);
    }
}

