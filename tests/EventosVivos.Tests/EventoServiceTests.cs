using EventosVivos.Application.DTOs;
using EventosVivos.Application.Services;
using EventosVivos.Domain.Entities;
using EventosVivos.Domain.Enums;
using EventosVivos.Domain.Exceptions;
using EventosVivos.Domain.Interfaces;
using Moq;

namespace EventosVivos.Tests;

public class EventoServiceTests
{
    private readonly Mock<IEventoRepository> _eventoRepoMock;
    private readonly Mock<IVenueRepository> _venueRepoMock;
    private readonly EventoService _service;

    public EventoServiceTests()
    {
        _eventoRepoMock = new Mock<IEventoRepository>();
        _venueRepoMock = new Mock<IVenueRepository>();
        _service = new EventoService(_eventoRepoMock.Object, _venueRepoMock.Object);
    }

    [Fact]
    public async Task CrearEvento_VenueNoExiste_LanzaNotFoundException()
    {
        // Given
        _venueRepoMock.Setup(x => x.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((Venue?)null);
        var request = new CrearEventoRequest("Evento Test 1", "Descripcion del evento test", 99, 100,
            DateTime.UtcNow.AddDays(7), DateTime.UtcNow.AddDays(7).AddHours(3), 50m, TipoEvento.Conferencia);

        // When / Then
        await Assert.ThrowsAsync<NotFoundException>(() => _service.CrearEventoAsync(request));
    }

    [Fact]
    public async Task CrearEvento_CapacidadExcedeVenue_LanzaBusinessRuleException()
    {
        // Given
        var venue = new Venue { Id = 1, Nombre = "Test Venue", Capacidad = 100, Ciudad = "Bogota" };
        _venueRepoMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(venue);
        var request = new CrearEventoRequest("Evento Test 2", "Descripcion del evento test", 1, 200,
            DateTime.UtcNow.AddDays(7), DateTime.UtcNow.AddDays(7).AddHours(3), 50m, TipoEvento.Conferencia);

        // When / Then
        var ex = await Assert.ThrowsAsync<BusinessRuleException>(() => _service.CrearEventoAsync(request));
        Assert.Contains("excede la capacidad", ex.Message);
    }

    [Fact]
    public async Task CrearEvento_FechaInicioPasada_LanzaBusinessRuleException()
    {
        // Given
        var venue = new Venue { Id = 1, Nombre = "Test Venue", Capacidad = 200, Ciudad = "Bogota" };
        _venueRepoMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(venue);
        var request = new CrearEventoRequest("Evento Test 3", "Descripcion del evento test", 1, 100,
            DateTime.UtcNow.AddHours(-1), DateTime.UtcNow.AddHours(2), 50m, TipoEvento.Conferencia);

        // When / Then
        var ex = await Assert.ThrowsAsync<BusinessRuleException>(() => _service.CrearEventoAsync(request));
        Assert.Contains("fecha de inicio debe ser futura", ex.Message);
    }

    [Fact]
    public async Task CrearEvento_FechaFinAnteriorAInicio_LanzaBusinessRuleException()
    {
        // Given
        var venue = new Venue { Id = 1, Nombre = "Test Venue", Capacidad = 200, Ciudad = "Bogota" };
        _venueRepoMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(venue);
        var fechaInicio = DateTime.UtcNow.AddDays(7);
        var request = new CrearEventoRequest("Evento Test 4", "Descripcion del evento test", 1, 100,
            fechaInicio, fechaInicio.AddHours(-1), 50m, TipoEvento.Conferencia);

        // When / Then
        var ex = await Assert.ThrowsAsync<BusinessRuleException>(() => _service.CrearEventoAsync(request));
        Assert.Contains("posterior a la fecha de inicio", ex.Message);
    }

    [Fact]
    public async Task CrearEvento_WeekendDespuesDe22_LanzaBusinessRuleException()
    {
        // Given
        var venue = new Venue { Id = 1, Nombre = "Test Venue", Capacidad = 200, Ciudad = "Bogota" };
        _venueRepoMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(venue);

        // Find next Saturday
        var today = DateTime.UtcNow;
        var daysUntilSaturday = ((int)DayOfWeek.Saturday - (int)today.DayOfWeek + 7) % 7;
        if (daysUntilSaturday == 0) daysUntilSaturday = 7;
        var saturday = today.Date.AddDays(daysUntilSaturday).AddHours(22).AddMinutes(30);

        var request = new CrearEventoRequest("Evento Nocturno", "Descripcion del evento nocturno", 1, 100,
            saturday, saturday.AddHours(3), 50m, TipoEvento.Concierto);

        // When / Then
        var ex = await Assert.ThrowsAsync<BusinessRuleException>(() => _service.CrearEventoAsync(request));
        Assert.Contains("22:00", ex.Message);
    }

    [Fact]
    public async Task CrearEvento_Superposicion_LanzaBusinessRuleException()
    {
        // Given
        var venue = new Venue { Id = 1, Nombre = "Test Venue", Capacidad = 200, Ciudad = "Bogota" };
        _venueRepoMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(venue);
        _eventoRepoMock.Setup(x => x.ExisteSuperposicionAsync(1, It.IsAny<DateTime>(), It.IsAny<DateTime>(), null))
            .ReturnsAsync(true);

        var fechaInicio = DateTime.UtcNow.AddDays(10);
        var request = new CrearEventoRequest("Evento Overlap", "Descripcion del evento overlap", 1, 100,
            fechaInicio, fechaInicio.AddHours(3), 50m, TipoEvento.Conferencia);

        // When / Then
        var ex = await Assert.ThrowsAsync<BusinessRuleException>(() => _service.CrearEventoAsync(request));
        Assert.Contains("superpuestos", ex.Message);
    }

    [Fact]
    public async Task CrearEvento_DatosValidos_RetornaEventoResponse()
    {
        // Given
        var venue = new Venue { Id = 1, Nombre = "Auditorio Central", Capacidad = 200, Ciudad = "Bogota" };
        _venueRepoMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(venue);
        _eventoRepoMock.Setup(x => x.ExisteSuperposicionAsync(1, It.IsAny<DateTime>(), It.IsAny<DateTime>(), null))
            .ReturnsAsync(false);
        _eventoRepoMock.Setup(x => x.CreateAsync(It.IsAny<Evento>()))
            .ReturnsAsync((Evento e) => { e.Id = 1; return e; });

        var fechaInicio = DateTime.UtcNow.AddDays(10);
        var request = new CrearEventoRequest("Conferencia Tech", "Una conferencia de tecnologia", 1, 150,
            fechaInicio, fechaInicio.AddHours(4), 75m, TipoEvento.Conferencia);

        // When
        var result = await _service.CrearEventoAsync(request);

        // Then
        Assert.Equal("Conferencia Tech", result.Titulo);
        Assert.Equal("activo", result.Estado);
        Assert.Equal("conferencia", result.TipoEvento);
    }
}

