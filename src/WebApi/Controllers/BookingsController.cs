using Application.Bookings.Cancel;
using Application.Bookings.Create;
using Application.Bookings.GetAll;
using Application.Bookings.GetById;
using Application.Common.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using WebApi.Contracts.Bookings;
using WebApi.Infrastructure;

namespace WebApi.Controllers;

[ApiController]
[Route("api/bookings")]
[Authorize]
public sealed class BookingsController : ControllerBase
{
	/// <summary>
	/// Створює бронювання конференц-залу.
	/// </summary>
	/// <remarks>
	/// Перед створенням бронювання система:
	///
	/// - перевіряє існування залу;
	/// - перевіряє доступність залу у заданий час;
	/// - перевіряє доступність вибраних послуг для залу;
	/// - розраховує вартість оренди відповідно до часових тарифів;
	/// - додає вартість вибраних додаткових послуг.
	///
	/// Час бронювання зберігається в UTC, а тарифікація виконується
	/// відповідно до часової зони бізнесу.
	/// </remarks>
	[HttpPost]
	[Authorize(Roles = Roles.Client)]
	[EnableRateLimiting(RateLimitingPolicies.Booking)]
	[ProducesResponseType<CreateBookingResult>(
		StatusCodes.Status201Created)]
	[ProducesResponseType<ValidationProblemDetails>(
		StatusCodes.Status400BadRequest)]
	[ProducesResponseType<ProblemDetails>(
		StatusCodes.Status404NotFound)]
	[ProducesResponseType<ProblemDetails>(
		StatusCodes.Status409Conflict)]
	[ProducesResponseType(StatusCodes.Status429TooManyRequests)]
	public async Task<ActionResult<CreateBookingResult>> Create(
		CreateBookingRequest request,
		[FromServices] CreateBookingHandler handler,
		CancellationToken cancellationToken)
	{
		var command = new CreateBookingCommand(
			request.ConferenceRoomId,
			request.StartTime,
			request.EndTime,
			request.ServiceIds);

		var result = await handler.HandleAsync(
			command,
			cancellationToken);

		return StatusCode(
			StatusCodes.Status201Created,
			result);
	}
	
	/// <summary>
	/// Повертає доступні поточному користувачу бронювання.
	/// </summary>
	/// <remarks>
	/// Client бачить лише власні бронювання.
	/// Admin бачить усі бронювання.
	/// </remarks>
	[HttpGet]
	[ProducesResponseType<IReadOnlyCollection<BookingListItemResult>>(
		StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status401Unauthorized)]
	public async Task<ActionResult<IReadOnlyCollection<BookingListItemResult>>> GetAll(
		[FromServices] GetBookingsHandler handler,
		CancellationToken cancellationToken)
	{
		var result = await handler.HandleAsync(
			cancellationToken);

		return Ok(result);
	}
	
	/// <summary>
	/// Повертає бронювання за ідентифікатором.
	/// </summary>
	[HttpGet("{id:guid}")]
	[ProducesResponseType<BookingDetailsResult>(
		StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status401Unauthorized)]
	[ProducesResponseType<ProblemDetails>(
		StatusCodes.Status404NotFound)]
	public async Task<ActionResult<BookingDetailsResult>> GetById(
		Guid id,
		[FromServices] GetBookingByIdHandler handler,
		CancellationToken cancellationToken)
	{
		var result = await handler.HandleAsync(
			id,
			cancellationToken);

		return Ok(result);
	}
	
	/// <summary>
	/// Скасовує бронювання.
	/// </summary>
	/// <remarks>
	/// Запис не видаляється з БД. Після скасування часовий
	/// проміжок знову стає доступним для нового бронювання.
	/// </remarks>
	[HttpPost("{id:guid}/cancel")]
	[ProducesResponseType(StatusCodes.Status204NoContent)]
	[ProducesResponseType(StatusCodes.Status401Unauthorized)]
	[ProducesResponseType<ProblemDetails>(
		StatusCodes.Status404NotFound)]
	[ProducesResponseType<ProblemDetails>(
		StatusCodes.Status409Conflict)]
	public async Task<IActionResult> Cancel(
		Guid id,
		[FromServices] CancelBookingHandler handler,
		CancellationToken cancellationToken)
	{
		await handler.HandleAsync(
			id,
			cancellationToken);

		return NoContent();
	}
}