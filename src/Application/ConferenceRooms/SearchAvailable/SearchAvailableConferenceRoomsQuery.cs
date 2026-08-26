namespace Application.ConferenceRooms.SearchAvailable;

/// <summary>
/// Містить критерії пошуку конференц-залів,
/// доступних у заданий часовий проміжок.
/// </summary>
public sealed record SearchAvailableConferenceRoomsQuery(
		DateTimeOffset StartTime,
		DateTimeOffset EndTime,
		int Capacity);