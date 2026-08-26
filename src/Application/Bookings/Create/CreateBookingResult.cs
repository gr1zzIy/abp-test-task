namespace Application.Bookings.Create;

public sealed record CreateBookingResult(
		Guid Id,
		decimal TotalPrice);