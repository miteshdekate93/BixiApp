using BixiApi.Models;
using BixiApi.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BixiApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StationsController : ControllerBase
{
    private readonly IBixiService _bixiService;

    public StationsController(IBixiService bixiService)
    {
        _bixiService = bixiService;
    }

    [HttpGet]
    public async Task<IActionResult> GetStations(
        [FromQuery] int? maxDistanceMeters,
        CancellationToken ct)
    {
        var stations = await _bixiService.GetStationsAsync(maxDistanceMeters, ct);
        return Ok(stations);
    }
}
