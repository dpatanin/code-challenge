using ADITUS.CodeChallenge.API.Domain;

namespace ADITUS.CodeChallenge.API.Services
{
  public interface IHardwareReservationService
  {
    Task<bool> ReserveHardware(HardwareReservationRequest request);
    Task<List<HardwareReservationStatus>> GetHardwareReservationStatus(Guid eventId);
  }
}
