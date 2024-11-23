using ADITUS.CodeChallenge.API.Domain;

namespace ADITUS.CodeChallenge.API.Services
{
  public interface IEventService
  {
    Task<Event> GetEvent(Guid id);
    Task<IList<Event>> GetEvents();
    Task<IList<Event>> GetEventsWithStatistics();
    Task<Event> GetEventWithStatistics(Guid id);

    Task<EventStatistics> FetchAndMergeStatistics(Guid id, EventType type);
    Task<bool> ReserveHardware(HardwareReservationRequest request);
    Task<List<HardwareReservationStatus>> GetHardwareReservationStatus(Guid eventId);
  }
}