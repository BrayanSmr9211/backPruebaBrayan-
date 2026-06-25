using EventosVivos.Application.DTOs;
using EventosVivos.Application.Interfaces;
using EventosVivos.Domain.Entities;
using EventosVivos.Domain.Enums;
using EventosVivos.Domain.Exceptions;
using EventosVivos.Domain.Interfaces;

namespace EventosVivos.Application.Services;

public class EventoService : IEventoService
{
    private readonly IEventoRepository _eventoRepo;
    private readonly IVenueRepository _venueRepo;

    public EventoService(IEventoRepository eventoRepo, IVenueRepository venueRepo)
    {
        _eventoRepo = eventoRepo;
        _venueRepo = venueRepo;
    }

    public async Task<EventoResponse> CrearEventoAsync(CrearEventoRequest request)
    {
        var venue = await _venueRepo.GetByIdAsync(request.VenueId)
            ?? throw new NotFoundException($"Venue con Id {request.VenueId} no encontrado.");

        // RN-01: Capacidad no puede exceder la del venue
        if (request.CapacidadMaxima > venue.Capacidad)
            throw new BusinessRuleException(
                $"La capacidad maxima ({request.CapacidadMaxima}) excede la capacidad del venue ({venue.Capacidad}).");

        // Validar fechas
        if (request.FechaInicio <= DateTime.UtcNow)
            throw new BusinessRuleException("La fecha de inicio debe ser futura.");

        if (request.FechaFin <= request.FechaInicio)
            throw new BusinessRuleException("La fecha de fin debe ser posterior a la fecha de inicio.");

        // RN-03: Restriccion horario nocturno en weekends
        var diaSemana = request.FechaInicio.DayOfWeek;
        if ((diaSemana == DayOfWeek.Saturday || diaSemana == DayOfWeek.Sunday)
            && request.FechaInicio.Hour >= 22)
            throw new BusinessRuleException("Eventos en fines de semana no pueden iniciar despues de las 22:00.");

        // RN-02: Superposicion de venues
        var existeSuperposicion = await _eventoRepo.ExisteSuperposicionAsync(
            request.VenueId, request.FechaInicio, request.FechaFin);
        if (existeSuperposicion)
            throw new BusinessRuleException("Ya existe un evento activo en este venue con horarios superpuestos.");

        var evento = new Evento
        {
            Titulo = request.Titulo,
            Descripcion = request.Descripcion,
            VenueId = request.VenueId,
            CapacidadMaxima = request.CapacidadMaxima,
            FechaInicio = request.FechaInicio,
            FechaFin = request.FechaFin,
            PrecioEntrada = request.PrecioEntrada,
            TipoEvento = request.TipoEvento,
            Estado = EstadoEvento.Activo
        };

        var created = await _eventoRepo.CreateAsync(evento);

        return new EventoResponse(
            created.Id, created.Titulo, created.Descripcion,
            created.VenueId, venue.Nombre, created.CapacidadMaxima,
            created.FechaInicio, created.FechaFin, created.PrecioEntrada,
            created.TipoEvento.ToString().ToLower(), created.Estado.ToString().ToLower());
    }

    public async Task<IEnumerable<EventoResponse>> ListarEventosAsync(EventoFilterRequest filter)
    {
        var eventos = await _eventoRepo.GetAllAsync(
            filter.TipoEvento, filter.FechaDesde, filter.FechaHasta,
            filter.VenueId, filter.Estado, filter.Titulo);

        var result = new List<EventoResponse>();
        foreach (var e in eventos)
        {
            // RN-06: Actualizar estado si el evento ya termino
            if (e.Estado == EstadoEvento.Activo && DateTime.UtcNow > e.FechaFin)
            {
                e.Estado = EstadoEvento.Completado;
                await _eventoRepo.UpdateAsync(e);
            }

            result.Add(new EventoResponse(
                e.Id, e.Titulo, e.Descripcion,
                e.VenueId, e.Venue?.Nombre ?? string.Empty, e.CapacidadMaxima,
                e.FechaInicio, e.FechaFin, e.PrecioEntrada,
                e.TipoEvento.ToString().ToLower(), e.Estado.ToString().ToLower()));
        }

        return result;
    }

    public async Task<ReporteOcupacionResponse> ObtenerReporteOcupacionAsync(int eventoId)
    {
        var evento = await _eventoRepo.GetByIdWithReservasAsync(eventoId)
            ?? throw new NotFoundException($"Evento con Id {eventoId} no encontrado.");

        // RN-06: Actualizar estado automaticamente
        if (evento.Estado == EstadoEvento.Activo && DateTime.UtcNow > evento.FechaFin)
        {
            evento.Estado = EstadoEvento.Completado;
            await _eventoRepo.UpdateAsync(evento);
        }

        var vendidas = evento.EntradasVendidas();
        var perdidas = evento.EntradasPerdidas();
        var disponibles = evento.CapacidadMaxima - vendidas - perdidas;
        var porcentaje = evento.CapacidadMaxima > 0
            ? Math.Round((decimal)vendidas / evento.CapacidadMaxima * 100, 2)
            : 0;
        var ingresos = vendidas * evento.PrecioEntrada;

        return new ReporteOcupacionResponse(
            evento.Id, evento.Titulo, vendidas, disponibles,
            porcentaje, ingresos, evento.Estado.ToString().ToLower());
    }
}

