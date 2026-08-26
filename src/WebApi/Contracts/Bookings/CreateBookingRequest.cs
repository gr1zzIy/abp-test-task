namespace WebApi.Contracts.Bookings;

public sealed record CreateBookingRequest(
		Guid ConferenceRoomId,
		DateTimeOffset StartTime,
		DateTimeOffset EndTime,
		IReadOnlyCollection<int> ServiceIds);