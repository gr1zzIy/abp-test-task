namespace Application.ConferenceRooms.GetAll;

public sealed record GetConferenceRoomsResult(
    Guid Id,
    string Name,
    int Capacity,
    decimal HourlyRate,
    IReadOnlyCollection<ConferenceRoomServiceResult> Services);

public sealed record ConferenceRoomServiceResult(
    int Id,
    string Name,
    decimal Price);