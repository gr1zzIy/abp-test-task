namespace WebApi.Contracts.ConferenceRooms;

public sealed record UpdateConferenceRoomRequest(
    string Name,
    int Capacity,
    decimal HourlyRate,
    IReadOnlyCollection<int> ServiceIds);