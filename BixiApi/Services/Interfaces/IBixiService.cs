using BixiApi.Models;

namespace BixiApi.Services.Interfaces;

public interface IBixiService
{
    Task<IEnumerable<StationResult>> GetStationsAsync(int? maxDistanceMeters, CancellationToken ct);
}
