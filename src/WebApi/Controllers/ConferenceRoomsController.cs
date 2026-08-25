using Application.ConferenceRooms.Create;
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

        return StatusCode(
            StatusCodes.Status201Created,
            result);
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
}