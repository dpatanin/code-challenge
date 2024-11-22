using System.Text.Json.Serialization;

namespace ADITUS.CodeChallenge.API.Domain
{
  public class OnlineStatistics
  {
    [JsonPropertyName("attendees")]
    public int Attendees { get; set; }

    [JsonPropertyName("invites")]
    public int Invites { get; set; }

    [JsonPropertyName("visits")]
    public int Visits { get; set; }

    [JsonPropertyName("virtualRooms")]
    public int VirtualRooms { get; set; }
  }
}
