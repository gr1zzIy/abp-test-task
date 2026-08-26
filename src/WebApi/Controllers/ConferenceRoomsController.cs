using Application.ConferenceRooms.Create;
using Application.ConferenceRooms.Delete;
using Application.ConferenceRooms.GetAll;
using Application.ConferenceRooms.GetById;
using Application.ConferenceRooms.SearchAvailable;
using Application.ConferenceRooms.Update;
using Microsoft.AspNetCore.Mvc;
using WebApi.Contracts.ConferenceRooms;

namespace WebApi.Controllers;

[ApiController]
[Route("api/conference-rooms")]
public class ConferenceRoomsController : ControllerBase
{
    [HttpPost]
    [ProducesResponseType<CreateConferenceRoomResult>(
        StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(
        CreateConferenceRoomRequest request,
        [FromServices] CreateConferenceRoomHandler handler,
        CancellationToken cancellationToken)
    {
        var command = new CreateConferenceRoomCommand(
            request.Name,
            request.Capacity,
            request.HourlyRate,
            request.ServiceIds);

        var result = await handler.HandleAsync(
            command,
            cancellationToken);

        return CreatedAtAction(
            nameof(GetById),
            new { id = result.Id },
            result);
    }
    
    [HttpGet]
    [ProducesResponseType<IReadOnlyCollection<GetConferenceRoomsResult>>(
        StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyCollection<GetConferenceRoomsResult>>> GetAll(
        [FromServices] GetConferenceRoomsHandler handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(cancellationToken);

        return Ok(result);
    }
    
    [HttpGet("{id:guid}")]
    [ProducesResponseType<GetConferenceRoomByIdResult>(
        StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<GetConferenceRoomByIdResult>> GetById(
        Guid id,
        [FromServices] GetConferenceRoomByIdHandler handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(
            id,
            cancellationToken);

        return Ok(result);
    }
    
    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update(
        Guid id,
        UpdateConferenceRoomRequest request,
        [FromServices] UpdateConferenceRoomHandler handler,
        CancellationToken cancellationToken)
    {
        var command = new UpdateConferenceRoomCommand(
            id,
            request.Name,
            request.Capacity,
            request.HourlyRate,
            request.ServiceIds);

        await handler.HandleAsync(
            command,
            cancellationToken);

        return NoContent();
    }
    
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(
        Guid id,
        [FromServices] DeleteConferenceRoomHandler handler,
        CancellationToken cancellationToken)
    {
        await handler.HandleAsync(
            id,
            cancellationToken);

        return NoContent();
    }
    
    [HttpGet("available")]
    [ProducesResponseType<IReadOnlyCollection<AvailableConferenceRoomResult>>(
    StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IReadOnlyCollection<AvailableConferenceRoomResult>>> GetAvailable(
            [FromQuery] DateTimeOffset startTime,
            [FromQuery] DateTimeOffset endTime,
            [FromQuery] int capacity,
            [FromServices] SearchAvailableConferenceRoomsHandler handler,
            CancellationToken cancellationToken)
    {
        var query = new SearchAvailableConferenceRoomsQuery(
        startTime,
        endTime,
        capacity);

        var result = await handler.HandleAsync(
        query,
        cancellationToken);

        return Ok(result);
    }
}