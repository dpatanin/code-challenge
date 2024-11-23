using ADITUS.CodeChallenge.API.Domain;

namespace ADITUS.CodeChallenge.API.Services
{
  public interface IHardwareReservationService
  {
    Task<bool> ReserveHardware(HardwareReservationRequest request);
    Task<HardwareReservationStatus> GetHardwareReservationStatus(Guid eventId);
  }
}
