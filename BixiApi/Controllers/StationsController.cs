using BixiApi.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BixiApi.Controllers;

/// <summary>
/// Exposes the single endpoint the frontend calls to get nearby BIXI stations.
///
/// This controller is intentionally thin — it only validates input and
/// delegates all business logic to IBixiService. Controllers should not
/// contain business logic; that belongs in the service layer.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class StationsController : ControllerBase
{
    private readonly IBixiService _bixiService;

    public StationsController(IBixiService bixiService)
    {
        _bixiService = bixiService;
    }

    /// <summary>
    /// Returns BIXI stations near the Workleap office, sorted by distance.
    /// Optionally filters to only stations within a given radius.
    /// </summary>
    /// <param name="maxDistanceMeters">
    /// Straight-line distance cap in meters. Must be > 0 when provided.
    /// Omit to return all stations regardless of distance.
    /// </param>
    [HttpGet]
    public async Task<IActionResult> GetStations([FromQuery] int? maxDistanceMeters)
    {
        // Validate early: 0 or negative is not a meaningful distance.
        // We return 400 here rather than letting the service deal with it.
        if (maxDistanceMeters.HasValue && maxDistanceMeters.Value <= 0)
            return BadRequest("maxDistanceMeters must be greater than 0");

        // Pass the request's cancellation token so that if the user closes
        // the browser tab, the BIXI API calls are cancelled too — avoids wasted work.
        var results = await _bixiService.GetStationsAsync(
            maxDistanceMeters, HttpContext.RequestAborted);

        return Ok(results);
    }
}
