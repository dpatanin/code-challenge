using Swashbuckle.AspNetCore.Annotations;
using ADITUS.CodeChallenge.API.Domain;
using ADITUS.CodeChallenge.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace ADITUS.CodeChallenge.API
{
  [Route("events")]
  [ApiController]
  public class EventsController : ControllerBase
  {
    private readonly IEventService _eventService;
    private readonly IHardwareReservationService _hardwareReservationService;

    public EventsController(IEventService eventService, IHardwareReservationService hardwareReservationService)
    {
      _eventService = eventService;
      _hardwareReservationService = hardwareReservationService;
    }

    /// <summary>
    /// Get all events with their statistics.
    /// </summary>
    /// <returns>List of events with their statistics.</returns>
    /// <response code="200">Returns list of events.</response>
    [HttpGet]
    [Route("")]
    [SwaggerOperation(Summary = "Get all events with statistics")]
    [ProducesResponseType(typeof(IList<Event>), 200)]
    public async Task<IActionResult> GetEvents()
    {
      var events = await _eventService.GetEventsWithStatistics();
      return Ok(events);
    }

    /// <summary>
    /// Get an event by ID with statistics.
    /// </summary>
    /// <param name="id">Event unique identifier</param>
    /// <returns>An event object with statistics</returns>
    /// <response code="200">Returns the event with statistics.</response>
    /// <response code="404">Event not found</response>
    [HttpGet]
    [Route("{id}")]
    [SwaggerOperation(Summary = "Get event by ID with statistics")]
    [ProducesResponseType(typeof(Event), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetEvent(Guid id)
    {
      var @event = await _eventService.GetEventWithStatistics(id);
      if (@event == null)
      {
        return NotFound();
      }

      return Ok(@event);
    }

    /// <summary>
    /// Reserve hardware for the specified event.
    /// </summary>
    /// <param name="id">Event unique identifier</param>
    /// <param name="request">Hardware reservation request details</param>
    /// <returns>Reservation result message</returns>
    /// <response code="200">Hardware reservation successful and pending approval</response>
    /// <response code="400">Invalid hardware reservation request</response>
    [HttpPost]
    [Route("{id}/reserve-hardware")]
    [SwaggerOperation(Summary = "Reserve hardware for an event")]
    [ProducesResponseType(typeof(string), 200)]
    [ProducesResponseType(400)]
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

    /// <summary>
    /// Get the hardware reservation for a specific event.
    /// </summary>
    /// <param name="id">Event unique identifier</param>
    /// <returns>Hardware reservation for the event</returns>
    /// <response code="200">Returns a hardware reservation</response>
    /// <response code="404">No reservations found</response>
    [HttpGet]
    [Route("{id}/hardware-reservations")]
    [SwaggerOperation(Summary = "Get hardware reservation for an event")]
    [ProducesResponseType(typeof(HardwareReservationStatus), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetHardwareReservations(Guid id)
    {
      var reservation = await _hardwareReservationService.GetHardwareReservationStatus(id);
      if (reservation == null)
      {
        return NotFound("No reservations found for this event.");
      }
      
      return Ok(reservation);
    }
  }
}
