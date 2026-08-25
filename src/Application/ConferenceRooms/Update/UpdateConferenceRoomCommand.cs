namespace Application.ConferenceRooms.Update;

public sealed record UpdateConferenceRoomCommand(
    Guid Id,
    string Name,
    int Capacity,
    decimal HourlyRate,
    IReadOnlyCollection<int> ServiceIds);