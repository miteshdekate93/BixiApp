using BixiApi.Services.Interfaces;

namespace BixiApi.Services;

/// <summary>
/// Calculates straight-line ("as the crow flies") distance between two
/// geographic coordinates using the Haversine formula.
///
/// Why Haversine? The Earth is a sphere, so simple Euclidean distance is
/// inaccurate for geo-coordinates. Haversine accounts for the Earth's
/// curvature and is accurate enough for distances up to a few hundred km.
/// </summary>
public class DistanceService : IDistanceService
{
    // Mean radius of the Earth in meters — standard value used by mapping systems.
    private const double EarthRadiusMeters = 6371000;

    /// <summary>
    /// Returns the distance in meters between two points on the Earth's surface.
    /// All coordinate inputs are in decimal degrees (WGS-84).
    /// </summary>
    public double Calculate(double lat1, double lon1, double lat2, double lon2)
    {
        // Convert the angular difference between the two points to radians.
        // Haversine operates in radians, not degrees.
        var dLat = ToRadians(lat2 - lat1);
        var dLon = ToRadians(lon2 - lon1);

        // Haversine formula:
        // a = sin²(Δlat/2) + cos(lat1) * cos(lat2) * sin²(Δlon/2)
        // This is the square of half the chord length between the two points.
        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2)
              + Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2))
              * Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

        // c = 2 * atan2(√a, √(1−a))
        // This is the angular distance in radians between the two points.
        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

        // Multiply by Earth's radius to get the arc distance in meters.
        return EarthRadiusMeters * c;
    }

    private static double ToRadians(double degrees) => degrees * Math.PI / 180.0;
}
