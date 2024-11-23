using ADITUS.CodeChallenge.API.Domain;
using ADITUS.CodeChallenge.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace ADITUS.CodeChallenge.API
{
  [Route("events")]
  public class EventsController : ControllerBase
  {
    private readonly IEventService _eventService;

    public EventsController(IEventService eventService)
    {
      _eventService = eventService;
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

      var success = await _eventService.ReserveHardware(request);
      if (!success)
      {
        return BadRequest("Hardware reservation failed. Ensure the reservation meets the criteria.");
      }

      return Ok("Hardware reservation successful. Pending approval.");
    }

    [HttpGet]
    [Route("{id}/hardware-reservation-status")]
    public async Task<IActionResult> GetHardwareReservationStatus(Guid id)
    {
      var reservations = await _eventService.GetHardwareReservationStatus(id);
      if (reservations == null || !reservations.Any())
      {
        return NotFound("No reservations found for this event.");
      }

      return Ok(reservations);
    }
  }
}
