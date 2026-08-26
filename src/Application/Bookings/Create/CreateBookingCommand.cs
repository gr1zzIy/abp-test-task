namespace Application.Bookings.Create;

public sealed record CreateBookingCommand(
		Guid ConferenceRoomId,
		DateTimeOffset StartTime,
		DateTimeOffset EndTime,
		IReadOnlyCollection<int> ServiceIds);