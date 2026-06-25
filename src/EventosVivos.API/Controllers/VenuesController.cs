using EventosVivos.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace EventosVivos.API.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class VenuesController : ControllerBase
{
    private readonly IVenueRepository _venueRepo;

    public VenuesController(IVenueRepository venueRepo)
    {
        _venueRepo = venueRepo;
    }

    /// <summary>
    /// Lista todos los venues disponibles.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Listar()
    {
        var venues = await _venueRepo.GetAllAsync();
        return Ok(venues);
    }
}

