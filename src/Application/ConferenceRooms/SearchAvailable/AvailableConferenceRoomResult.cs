namespace Application.ConferenceRooms.SearchAvailable;

public sealed record AvailableConferenceRoomResult(
		Guid Id,
		string Name,
		int Capacity,
		decimal HourlyRate,
		IReadOnlyCollection<AvailableConferenceRoomServiceResult> Services);

public sealed record AvailableConferenceRoomServiceResult(
		int Id,
		string Name,
		decimal Price);