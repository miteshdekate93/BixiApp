namespace BixiApi.Models;

public class StationResult
{
    public string StationId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int AvailableBikes { get; set; }
    public double DistanceMeters { get; set; }
    public bool IsAvailable { get; set; }
}
