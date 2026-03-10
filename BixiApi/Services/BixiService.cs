using BixiApi.Models;
using BixiApi.Services.Interfaces;

namespace BixiApi.Services;

public class BixiService : IBixiService
{
    public Task<IEnumerable<StationResult>> GetStationsAsync(int? maxDistanceMeters, CancellationToken ct)
    {
        return Task.FromResult(Enumerable.Empty<StationResult>());
    }
}
