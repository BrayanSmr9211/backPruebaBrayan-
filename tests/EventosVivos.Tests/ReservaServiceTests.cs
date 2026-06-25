using EventosVivos.Application.DTOs;
using EventosVivos.Application.Services;
using EventosVivos.Domain.Entities;
using EventosVivos.Domain.Enums;
using EventosVivos.Domain.Exceptions;
using EventosVivos.Domain.Interfaces;
using Moq;

namespace EventosVivos.Tests;

public class ReservaServiceTests
{
    private readonly Mock<IReservaRepository> _reservaRepoMock;
    private readonly Mock<IEventoRepository> _eventoRepoMock;
    private readonly ReservaService _service;

    public ReservaServiceTests()
    {
        _reservaRepoMock = new Mock<IReservaRepository>();
        _eventoRepoMock = new Mock<IEventoRepository>();
        _service = new ReservaService(_reservaRepoMock.Object, _eventoRepoMock.Object);
    }

    private Evento CrearEventoActivo(int capacidad = 100, decimal precio = 50m, double horasParaInicio = 48)
    {
        return new Evento
        {
            Id = 1,
            Titulo = "Evento Test",
            CapacidadMaxima = capacidad,
            PrecioEntrada = precio,
            Estado = EstadoEvento.Activo,
            FechaInicio = DateTime.UtcNow.AddHours(horasParaInicio),
            FechaFin = DateTime.UtcNow.AddHours(horasParaInicio + 3),
            Reservas = new List<Reserva>()
        };
    }

    [Fact]
    public async Task CrearReserva_EventoNoExiste_LanzaNotFoundException()
    {
        // Given
        _eventoRepoMock.Setup(x => x.GetByIdWithReservasAsync(It.IsAny<int>())).ReturnsAsync((Evento?)null);
        var request = new CrearReservaRequest(99, 2, "Juan", "juan@test.com");

        // When / Then
        await Assert.ThrowsAsync<NotFoundException>(() => _service.CrearReservaAsync(request));
    }

    [Fact]
    public async Task CrearReserva_EventoNoActivo_LanzaBusinessRuleException()
    {
        // Given
        var evento = CrearEventoActivo();
        evento.Estado = EstadoEvento.Cancelado;
        _eventoRepoMock.Setup(x => x.GetByIdWithReservasAsync(1)).ReturnsAsync(evento);
        var request = new CrearReservaRequest(1, 2, "Juan", "juan@test.com");

        // When / Then
        var ex = await Assert.ThrowsAsync<BusinessRuleException>(() => _service.CrearReservaAsync(request));
        Assert.Contains("activos", ex.Message);
    }

    [Fact]
    public async Task CrearReserva_MenosDe1Hora_LanzaBusinessRuleException()
    {
        // Given - RN-04
        var evento = CrearEventoActivo(horasParaInicio: 0.5);
        _eventoRepoMock.Setup(x => x.GetByIdWithReservasAsync(1)).ReturnsAsync(evento);
        var request = new CrearReservaRequest(1, 2, "Juan", "juan@test.com");

        // When / Then
        var ex = await Assert.ThrowsAsync<BusinessRuleException>(() => _service.CrearReservaAsync(request));
        Assert.Contains("menos de 1 hora", ex.Message);
    }

    [Fact]
    public async Task CrearReserva_MenosDe24Horas_MaximoPermitido5_LanzaBusinessRuleException()
    {
        // Given - RF-03 prioridad sobre RN-05
        var evento = CrearEventoActivo(horasParaInicio: 12);
        _eventoRepoMock.Setup(x => x.GetByIdWithReservasAsync(1)).ReturnsAsync(evento);
        var request = new CrearReservaRequest(1, 6, "Juan", "juan@test.com");

        // When / Then
        var ex = await Assert.ThrowsAsync<BusinessRuleException>(() => _service.CrearReservaAsync(request));
        Assert.Contains("maximo 5 entradas", ex.Message);
    }

    [Fact]
    public async Task CrearReserva_PrecioMayorA100_Maximo10Entradas_LanzaBusinessRuleException()
    {
        // Given - RN-05
        var evento = CrearEventoActivo(precio: 150m, horasParaInicio: 72);
        _eventoRepoMock.Setup(x => x.GetByIdWithReservasAsync(1)).ReturnsAsync(evento);
        var request = new CrearReservaRequest(1, 11, "Juan", "juan@test.com");

        // When / Then
        var ex = await Assert.ThrowsAsync<BusinessRuleException>(() => _service.CrearReservaAsync(request));
        Assert.Contains("maximo 10 entradas", ex.Message);
    }

    [Fact]
    public async Task CrearReserva_SinDisponibilidad_LanzaBusinessRuleException()
    {
        // Given
        var evento = CrearEventoActivo(capacidad: 5);
        evento.Reservas = new List<Reserva>
        {
            new Reserva { Id = 1, Cantidad = 5, Estado = EstadoReserva.Confirmada }
        };
        _eventoRepoMock.Setup(x => x.GetByIdWithReservasAsync(1)).ReturnsAsync(evento);
        var request = new CrearReservaRequest(1, 1, "Juan", "juan@test.com");

        // When / Then
        var ex = await Assert.ThrowsAsync<BusinessRuleException>(() => _service.CrearReservaAsync(request));
        Assert.Contains("No hay suficientes entradas", ex.Message);
    }

    [Fact]
    public async Task CrearReserva_Exitosa_RetornaReservaResponse()
    {
        // Given
        var evento = CrearEventoActivo();
        _eventoRepoMock.Setup(x => x.GetByIdWithReservasAsync(1)).ReturnsAsync(evento);
        _reservaRepoMock.Setup(x => x.CreateAsync(It.IsAny<Reserva>()))
            .ReturnsAsync((Reserva r) => { r.Id = 1; return r; });
        var request = new CrearReservaRequest(1, 3, "Juan Perez", "juan@test.com");

        // When
        var result = await _service.CrearReservaAsync(request);

        // Then
        Assert.Equal("pendientepago", result.Estado);
        Assert.Equal(3, result.Cantidad);
        Assert.Equal("Juan Perez", result.NombreComprador);
    }

    [Fact]
    public async Task ConfirmarPago_ReservaNoExiste_LanzaNotFoundException()
    {
        // Given
        _reservaRepoMock.Setup(x => x.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((Reserva?)null);

        // When / Then
        await Assert.ThrowsAsync<NotFoundException>(() => _service.ConfirmarPagoAsync(99));
    }

    [Fact]
    public async Task ConfirmarPago_YaConfirmada_LanzaBusinessRuleException()
    {
        // Given
        var reserva = new Reserva { Id = 1, Estado = EstadoReserva.Confirmada, Evento = new Evento { Titulo = "Test" } };
        _reservaRepoMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(reserva);

        // When / Then
        var ex = await Assert.ThrowsAsync<BusinessRuleException>(() => _service.ConfirmarPagoAsync(1));
        Assert.Contains("ya esta confirmada", ex.Message);
    }

    [Fact]
    public async Task ConfirmarPago_Cancelada_LanzaBusinessRuleException()
    {
        // Given
        var reserva = new Reserva { Id = 1, Estado = EstadoReserva.Cancelada, Evento = new Evento { Titulo = "Test" } };
        _reservaRepoMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(reserva);

        // When / Then
        var ex = await Assert.ThrowsAsync<BusinessRuleException>(() => _service.ConfirmarPagoAsync(1));
        Assert.Contains("cancelada", ex.Message);
    }

    [Fact]
    public async Task ConfirmarPago_Exitosa_GeneraCodigoReserva()
    {
        // Given
        var reserva = new Reserva { Id = 1, Estado = EstadoReserva.PendientePago, EventoId = 1, Evento = new Evento { Titulo = "Test" } };
        _reservaRepoMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(reserva);
        _reservaRepoMock.Setup(x => x.UpdateAsync(It.IsAny<Reserva>())).Returns(Task.CompletedTask);

        // When
        var result = await _service.ConfirmarPagoAsync(1);

        // Then
        Assert.Equal("confirmada", result.Estado);
        Assert.NotNull(result.CodigoReserva);
        Assert.StartsWith("EV-", result.CodigoReserva);
        Assert.Equal(9, result.CodigoReserva.Length);
    }

    [Fact]
    public async Task CancelarReserva_PendientePago_LanzaBusinessRuleException()
    {
        // Given
        var reserva = new Reserva { Id = 1, Estado = EstadoReserva.PendientePago, Evento = new Evento { Titulo = "Test" } };
        _reservaRepoMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(reserva);

        // When / Then
        var ex = await Assert.ThrowsAsync<BusinessRuleException>(() => _service.CancelarReservaAsync(1));
        Assert.Contains("pendiente_pago", ex.Message);
    }

    [Fact]
    public async Task CancelarReserva_MenosDe48Horas_MarcarPerdidas()
    {
        // Given - RN-07
        var evento = new Evento
        {
            Id = 1,
            Titulo = "Evento Pronto",
            FechaInicio = DateTime.UtcNow.AddHours(24)
        };
        var reserva = new Reserva { Id = 1, EventoId = 1, Estado = EstadoReserva.Confirmada, Evento = evento };
        _reservaRepoMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(reserva);
        _eventoRepoMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(evento);
        _reservaRepoMock.Setup(x => x.UpdateAsync(It.IsAny<Reserva>())).Returns(Task.CompletedTask);

        // When
        var result = await _service.CancelarReservaAsync(1);

        // Then
        Assert.Equal("cancelada", result.Estado);
        Assert.True(result.EntradasPerdidas);
        Assert.NotNull(result.FechaCancelacion);
    }

    [Fact]
    public async Task CancelarReserva_MasDe48Horas_NoMarcarPerdidas()
    {
        // Given
        var evento = new Evento
        {
            Id = 1,
            Titulo = "Evento Lejano",
            FechaInicio = DateTime.UtcNow.AddHours(72)
        };
        var reserva = new Reserva { Id = 1, EventoId = 1, Estado = EstadoReserva.Confirmada, Evento = evento };
        _reservaRepoMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(reserva);
        _eventoRepoMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(evento);
        _reservaRepoMock.Setup(x => x.UpdateAsync(It.IsAny<Reserva>())).Returns(Task.CompletedTask);

        // When
        var result = await _service.CancelarReservaAsync(1);

        // Then
        Assert.Equal("cancelada", result.Estado);
        Assert.False(result.EntradasPerdidas);
    }
}

