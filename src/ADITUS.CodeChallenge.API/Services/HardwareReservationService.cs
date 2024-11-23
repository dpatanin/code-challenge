using ADITUS.CodeChallenge.API.Domain;

namespace ADITUS.CodeChallenge.API.Services
{
  public class HardwareReservationService : IHardwareReservationService
  {
    private readonly IEventService _eventService;
    private readonly List<Hardware> _hardwareInventory;
    private readonly Dictionary<Guid, List<HardwareReservationStatus>> _reservations;
    private readonly int _minDaysBeforeEventForReservation = 28;

    public HardwareReservationService(IEventService eventService)
    {
      _eventService = eventService;
      _reservations = new Dictionary<Guid, List<HardwareReservationStatus>>();

      _hardwareInventory = new List<Hardware>
        {
            new Hardware { Name = "Drehsperre", AvailableQuantity = 100 },
            new Hardware { Name = "Funkhandscanner", AvailableQuantity = 200 },
            new Hardware { Name = "Mobiles Scan-Terminal", AvailableQuantity = 50 }
        };
    }

    public async Task<bool> ReserveHardware(HardwareReservationRequest request)
    {
      var @event = await _eventService.GetEvent(request.EventId);
      if (@event == null || !IsReservationEligible(@event) || !IsHardwareAvailable(request.RequestedHardware))
      {
        return false;
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

      return true;
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
