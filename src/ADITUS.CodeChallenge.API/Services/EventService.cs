using System.Text.Json;
using ADITUS.CodeChallenge.API.Domain;

namespace ADITUS.CodeChallenge.API.Services
{
  public class EventService : IEventService
  {
    private readonly int _minDaysBeforeEventForReservation = 28;
    private readonly IList<Event> _events;
    private readonly HttpClient _httpClient;
    private readonly List<Hardware> _hardwareInventory;
    private readonly Dictionary<Guid, List<HardwareReservationStatus>> _reservations;

    public EventService(HttpClient httpClient)
    {
      _httpClient = httpClient;
      _reservations = new Dictionary<Guid, List<HardwareReservationStatus>>();

      _hardwareInventory = new List<Hardware>
        {
            new Hardware { Name = "Drehsperre", AvailableQuantity = 100 },
            new Hardware { Name = "Funkhandscanner", AvailableQuantity = 200 },
            new Hardware { Name = "Mobiles Scan-Terminal", AvailableQuantity = 50 }
        };

      _events = new List<Event>
      {
        new Event
        {
          Id = Guid.Parse("7c63631c-18d4-4395-9c1e-886554265eb0"),
          Year = 2019,
          Name = "ADITUS Code Challenge 2019",
          StartDate = new DateTime(2019, 1, 1),
          EndDate = new DateTime(2019, 1, 31),
          Type = EventType.OnSite
        },
        new Event
        {
          Id = Guid.Parse("751fd775-2c8e-48e0-955c-2144008e984a"),
          Year = 2020,
          Name = "ADITUS Code Challenge 2020",
          StartDate = new DateTime(2020, 1, 1),
          EndDate = new DateTime(2020, 1, 15),
          Type = EventType.Hybrid
        },
        new Event
        {
          Id = Guid.Parse("974098e0-9b3f-41d5-80c2-551600ad204a"),
          Year = 2021,
          Name = "ADITUS Code Challenge 2021",
          StartDate = new DateTime(2021, 1, 1),
          EndDate = new DateTime(2021, 1, 18),
          Type = EventType.Online
        },
        new Event
        {
          Id = Guid.Parse("28669572-2b9a-4b2c-ad7e-6434ea8ab761"),
          Year = 2022,
          Name = "ADITUS Code Challenge 2022",
          StartDate = new DateTime(2022, 1, 1),
          EndDate = new DateTime(2022, 1, 11),
          Type = EventType.Online
        },
        new Event
        {
          Id = Guid.Parse("3a17b294-8716-448c-94db-ebf9bf53f1ce"),
          Year = 2023,
          Name = "ADITUS Code Challenge 2023",
          StartDate = new DateTime(2025, 1, 1),
          EndDate = new DateTime(2025, 1, 23),
          Type = EventType.OnSite
        }
      };
    }

    public Task<Event> GetEvent(Guid id)
    {
      var @event = _events.FirstOrDefault(e => e.Id == id);
      return Task.FromResult(@event);
    }

    public Task<IList<Event>> GetEvents()
    {
      return Task.FromResult(_events);
    }

    public async Task<IList<Event>> GetEventsWithStatistics()
    {
      var events = await GetEvents();
      foreach (var @event in events)
      {
        @event.Statistics = await FetchAndMergeStatistics(@event.Id, @event.Type);
      }
      return events;
    }

    public async Task<Event> GetEventWithStatistics(Guid id)
    {
      var @event = await GetEvent(id);
      if (@event != null)
      {
        @event.Statistics = await FetchAndMergeStatistics(@event.Id, @event.Type);
      }
      return @event;
    }

    public async Task<EventStatistics> FetchAndMergeStatistics(Guid id, EventType type)
    {
      var statistics = new EventStatistics();

      if (type == EventType.Online || type == EventType.Hybrid)
      {
        var onlineStatisticsUrl = $"https://codechallenge-statistics.azurewebsites.net/api/online-statistics/{id}";
        statistics.Online = await FetchOnlineStatistics(onlineStatisticsUrl);
      }

      if (type == EventType.OnSite || type == EventType.Hybrid)
      {
        var onsiteStatisticsUrl = $"https://codechallenge-statistics.azurewebsites.net/api/onsite-statistics/{id}";
        statistics.OnSite = await FetchOnSiteStatistics(onsiteStatisticsUrl);
      }

      return statistics;
    }

    private async Task<OnlineStatistics> FetchOnlineStatistics(string url)
    {
      try
      {
        var response = await _httpClient.GetAsync(url);

        if (response.IsSuccessStatusCode)
        {
          var content = await response.Content.ReadAsStringAsync();
          var statistics = JsonSerializer.Deserialize<OnlineStatistics>(content);
          return statistics;
        }
        else
        {
          return null;
        }
      }
      catch (Exception)
      {
        // Log errors if necessary
        return null;
      }
    }

    private async Task<OnSiteStatistics> FetchOnSiteStatistics(string url)
    {
      try
      {
        var response = await _httpClient.GetAsync(url);

        if (response.IsSuccessStatusCode)
        {
          var content = await response.Content.ReadAsStringAsync();
          var statistics = JsonSerializer.Deserialize<OnSiteStatistics>(content);
          return statistics;
        }
        else
        {
          return null;
        }
      }
      catch (Exception)
      {
        // Log errors if necessary
        return null;
      }
    }

    public Task<bool> ReserveHardware(HardwareReservationRequest request)
    {
      var @event = _events.FirstOrDefault(e => e.Id == request.EventId);
      if (@event == null || !IsReservationEligible(@event) || !IsHardwareAvailable(request.RequestedHardware))
      {
        return Task.FromResult(false);
      }

      foreach (var item in request.RequestedHardware)
      {
        var hardware = _hardwareInventory.First(h => h.Name == item.HardwareName);
        hardware.AvailableQuantity -= item.Quantity;
      }

      if (!_reservations.ContainsKey(request.EventId))
      {
        _reservations[request.EventId] = new List<HardwareReservationStatus>();
      }

      _reservations[request.EventId].Add(new HardwareReservationStatus
      {
        EventId = request.EventId,
        Status = ReservationStatus.Pending,
        ReservedHardware = request.RequestedHardware
      });

      return Task.FromResult(true);
    }

    private bool IsReservationEligible(Event @event)
    {
      return @event.StartDate != null
          && (@event.StartDate.Value - DateTime.Now).TotalDays >= _minDaysBeforeEventForReservation;
    }

    private bool IsHardwareAvailable(IEnumerable<HardwareRequestItem> requestedHardware)
    {
      return requestedHardware.All(item =>
      {
        var hardware = _hardwareInventory.FirstOrDefault(h => h.Name == item.HardwareName);
        return hardware != null && hardware.AvailableQuantity >= item.Quantity;
      });
    }

    public Task<List<HardwareReservationStatus>> GetHardwareReservationStatus(Guid eventId)
    {
      if (_reservations.TryGetValue(eventId, out var reservations))
      {
        return Task.FromResult(reservations);
      }

      return Task.FromResult(new List<HardwareReservationStatus>());
    }
  }
}