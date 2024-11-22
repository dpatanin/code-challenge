using System.Text.Json.Serialization;

namespace ADITUS.CodeChallenge.API.Domain
{
  public class OnSiteStatistics
  {
    [JsonPropertyName("visitorsCount")]
    public int VisitorsCount { get; set; }

    [JsonPropertyName("exhibitorsCount")]
    public int ExhibitorsCount { get; set; }

    [JsonPropertyName("boothsCount")]
    public int BoothsCount { get; set; }
  }
}
