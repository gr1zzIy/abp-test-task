using Application.Bookings.Create;
using Application.Common.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using WebApi.Contracts.Bookings;
using WebApi.Infrastructure;

namespace WebApi.Controllers;

[ApiController]
[Route("api/bookings")]
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
	[Authorize(Roles = Roles.Client)]
	[HttpPost]
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
}