namespace Application.Bookings.Create;

/// <summary>
/// Результат створення бронювання конференц-залу.
/// </summary>
/// <param name="Id">Ідентифікатор створеного бронювання.</param>
/// <param name="TotalPrice">Загальна вартість бронювання.</param>
public sealed record CreateBookingResult(
		Guid Id,
		decimal TotalPrice);