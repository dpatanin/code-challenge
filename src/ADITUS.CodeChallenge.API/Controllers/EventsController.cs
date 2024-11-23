using ADITUS.CodeChallenge.API.Domain;
using ADITUS.CodeChallenge.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace ADITUS.CodeChallenge.API
{
  [Route("events")]
  public class EventsController : ControllerBase
  {
    private readonly IEventService _eventService;
    private readonly IHardwareReservationService _hardwareReservationService;

    public EventsController(IEventService eventService, IHardwareReservationService hardwareReservationService)
    {
      _eventService = eventService;
      _hardwareReservationService = hardwareReservationService;
    }

    [HttpGet]
    [Route("")]
    public async Task<IActionResult> GetEvents()
    {
      var events = await _eventService.GetEventsWithStatistics();
      return Ok(events);
    }

    [HttpGet]
    [Route("{id}")]
    public async Task<IActionResult> GetEvent(Guid id)
    {
      var @event = await _eventService.GetEventWithStatistics(id);
      if (@event == null)
      {
        return NotFound();
      }

      return Ok(@event);
    }

    [HttpPost]
    [Route("{id}/reserve-hardware")]
    public async Task<IActionResult> ReserveHardware(Guid id, [FromBody] HardwareReservationRequest request)
    {
      request.EventId = id;

      var success = await _hardwareReservationService.ReserveHardware(request);
      if (!success)
      {
        return BadRequest("Hardware reservation failed. Ensure the reservation meets the criteria.");
      }

      return Ok("Hardware reservation successful. Pending approval.");
    }

    [HttpGet]
    [Route("{id}/hardware-reservations")]
    public async Task<IActionResult> GetHardwareReservations(Guid id)
    {
      var reservations = await _hardwareReservationService.GetHardwareReservationStatus(id);
      if (reservations.Any())
      {
        return Ok(reservations);
      }

      return NotFound("No reservations found for this event.");
    }
  }
}
