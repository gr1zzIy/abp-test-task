namespace Application.ConferenceRooms.Create;

/// <summary>
/// Результат створення конференц-залу.
/// </summary>
/// <param name="Id">Ідентифікатор створеного залу.</param>
public sealed record CreateConferenceRoomResult(Guid Id);