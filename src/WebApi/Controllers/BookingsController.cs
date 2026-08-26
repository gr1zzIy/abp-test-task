using Application.Bookings.Create;
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
	/// Створює бронювання конференц-залу та повертає
	/// розраховану загальну вартість оренди.
	/// </summary>
	[HttpPost]
	[EnableRateLimiting(RateLimitingPolicies.Booking)]
	[ProducesResponseType<CreateBookingResult>(
	StatusCodes.Status201Created)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	[ProducesResponseType(StatusCodes.Status409Conflict)]
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