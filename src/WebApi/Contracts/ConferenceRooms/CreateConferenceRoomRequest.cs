namespace WebApi.Contracts.ConferenceRooms;

public sealed record CreateConferenceRoomRequest(
    string Name,
    int Capacity,
    decimal HourlyRate,
    IReadOnlyCollection<int> ServiceIds);