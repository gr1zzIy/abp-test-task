namespace Application.ConferenceRooms.Create;

public sealed record CreateConferenceRoomCommand(
    string Name,
    int Capacity,
    decimal HourlyRate,
    IReadOnlyCollection<int> ServiceIds);