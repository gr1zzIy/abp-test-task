using Application.Common.Security;
using Application.ConferenceRooms.Create;
using Application.ConferenceRooms.Delete;
using Application.ConferenceRooms.GetAll;
using Application.ConferenceRooms.GetById;
using Application.ConferenceRooms.SearchAvailable;
using Application.ConferenceRooms.Update;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using WebApi.Contracts.ConferenceRooms;
using WebApi.Infrastructure;

namespace WebApi.Controllers;

[ApiController]
[Route("api/conference-rooms")]
public class ConferenceRoomsController : ControllerBase
{
    /// <summary>
    /// Створює новий конференц-зал.
    /// </summary>
    /// <remarks>
    /// Для залу задаються назва, місткість, базова погодинна
    /// вартість та перелік доступних додаткових послуг.
    /// </remarks>
    [Authorize(Roles = Roles.Admin)]
    [HttpPost]
    [EnableRateLimiting(RateLimitingPolicies.Write)]
    [ProducesResponseType<CreateConferenceRoomResult>(
        StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(
        StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(
        StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
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
    
    /// <summary>
    /// Повертає список усіх конференц-залів.
    /// </summary>
    /// <returns>
    /// Список конференц-залів із місткістю, базовою вартістю
    /// та доступними додатковими послугами.
    /// </returns>
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
    
    /// <summary>
    /// Повертає конференц-зал за його ідентифікатором.
    /// </summary>
    /// <param name="id">Ідентифікатор конференц-залу.</param>
    /// <returns>Дані знайденого конференц-залу.</returns>
    [HttpGet("{id:guid}")]
    [ProducesResponseType<GetConferenceRoomByIdResult>(
        StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(
        StatusCodes.Status404NotFound)]
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
    
    /// <summary>
    /// Оновлює інформацію про конференц-зал.
    /// </summary>
    /// <remarks>
    /// Дозволяє змінити назву, місткість, базову погодинну
    /// вартість та повний набір доступних послуг.
    /// </remarks>
    /// <param name="id">Ідентифікатор конференц-залу.</param>
    [Authorize(Roles = Roles.Admin)]
    [HttpPut("{id:guid}")]
    [EnableRateLimiting(RateLimitingPolicies.Write)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ValidationProblemDetails>(
        StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(
        StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(
        StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
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
    
    /// <summary>
    /// Видаляє конференц-зал.
    /// </summary>
    /// <remarks>
    /// Зал не може бути видалений, якщо з ним уже пов'язані бронювання.
    /// Це дозволяє зберегти історичні дані.
    /// </remarks>
    /// <param name="id">Ідентифікатор конференц-залу.</param>
    [Authorize(Roles = Roles.Admin)]
    [HttpDelete("{id:guid}")]
    [EnableRateLimiting(RateLimitingPolicies.Write)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(
        StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(
        StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
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
    
    /// <summary>
    /// Повертає конференц-зали, доступні у заданий часовий проміжок.
    /// </summary>
    /// <remarks>
    /// Зал вважається доступним, якщо його місткість не менша
    /// за вказану та він не має бронювань, що перетинаються
    /// із заданим часовим проміжком.
    /// </remarks>
    /// <param name="startTime">Дата та час початку бажаного бронювання.</param>
    /// <param name="endTime">Дата та час завершення бажаного бронювання.</param>
    /// <param name="capacity">Мінімальна необхідна місткість залу.</param>
    [HttpGet("available")]
    [ProducesResponseType<IReadOnlyCollection<AvailableConferenceRoomResult>>(
        StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(
        StatusCodes.Status400BadRequest)]
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