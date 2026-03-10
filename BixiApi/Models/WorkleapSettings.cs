namespace BixiApi.Models;

public class WorkleapSettings
{
    public double OfficeLat { get; set; }
    public double OfficeLng { get; set; }
    public string GbfsBaseUrl { get; set; } = string.Empty;
    public int HttpTimeoutSeconds { get; set; }
}
