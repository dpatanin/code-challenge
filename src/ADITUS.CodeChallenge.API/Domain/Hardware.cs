namespace ADITUS.CodeChallenge.API.Domain
{
  public class Hardware
  {
    public string Name { get; set; }
    public int AvailableQuantity { get; set; }
  }

  public class HardwareReservationRequest
  {
    public Guid EventId { get; set; }
    public List<HardwareRequestItem> RequestedHardware { get; set; }
  }

  public class HardwareRequestItem
  {
    public string HardwareName { get; set; }
    public int Quantity { get; set; }
  }

  public class HardwareReservationStatus
  {
    public Guid EventId { get; set; }
    public ReservationStatus Status { get; set; }
    public List<HardwareRequestItem> ReservedHardware { get; set; }
  }

  [Flags]
  public enum ReservationStatus
  {
    Pending,
    Approved,
    Rejected
  }
}
