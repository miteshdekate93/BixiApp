using BixiApi.Services;
using Xunit;

namespace BixiApi.Tests.ServiceTests;

/// <summary>
/// Unit tests for DistanceService (Haversine formula).
///
/// Each test covers a distinct property of the calculation:
/// - Edge case: zero distance for the same point
/// - Real-world accuracy: known Montreal landmarks
/// - Mathematical property: symmetry (A→B == B→A)
///
/// Tests follow the Arrange-Act-Assert pattern and are named using the
/// "MethodName_Scenario_ExpectedResult" convention for readability.
/// </summary>
public class DistanceServiceTests
{
    // "sut" = System Under Test — a common convention in unit testing.
    private readonly DistanceService _sut = new();

    [Fact]
    public void SamePoint_ReturnsZero()
    {
        // When both points are identical, all Haversine deltas are zero,
        // so the result must be exactly 0.0 — a good sanity check for the formula.
        var result = _sut.Calculate(45.484, -73.562, 45.484, -73.562);

        Assert.Equal(0.0, result);
    }

    [Fact]
    public void WorkleapToMetroLaurier_IsApproximately5100m()
    {
        // Real-world check: Workleap office (Mile-Ex) to Métro Laurier.
        // Validates the formula produces a plausible result against a known pair
        // of Montreal coordinates. We allow a ±250m tolerance for floating-point
        // and Earth-model variance.
        var result = _sut.Calculate(45.48415789031987, -73.56216762891964, 45.5275, -73.5858);

        Assert.InRange(result, 4900, 5400);
    }

    [Fact]
    public void OneDegreeNorth_IsApproximately111km()
    {
        // One degree of latitude always equals ~111 km regardless of longitude.
        // This is a well-known geographic constant and a strong correctness signal
        // for the Haversine implementation.
        var result = _sut.Calculate(45.484, -73.562, 46.484, -73.562);

        Assert.InRange(result, 110000, 112000);
    }

    [Fact]
    public void TwoNearbyPoints_AreApproximately500mApart()
    {
        // Short-range accuracy test: two points a small fraction of a degree apart
        // should produce a result consistent with walking/cycling distances.
        // This catches scale errors (e.g. returning km instead of meters).
        var result = _sut.Calculate(45.484, -73.562, 45.4885, -73.562);

        Assert.InRange(result, 450, 550);
    }

    [Fact]
    public void Distance_IsSymmetric()
    {
        // The Haversine formula is mathematically symmetric: distance A→B must
        // equal distance B→A. Floating-point arithmetic can introduce tiny
        // rounding differences, so we allow a sub-millimeter tolerance (0.001m).
        double lat1 = 45.484, lon1 = -73.562;
        double lat2 = 45.5275, lon2 = -73.5858;

        var ab = _sut.Calculate(lat1, lon1, lat2, lon2);
        var ba = _sut.Calculate(lat2, lon2, lat1, lon1);

        Assert.True(Math.Abs(ab - ba) < 0.001);
    }
}
