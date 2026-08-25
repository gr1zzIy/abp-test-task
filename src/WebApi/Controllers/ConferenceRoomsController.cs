using Application.ConferenceRooms.Create;
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
}