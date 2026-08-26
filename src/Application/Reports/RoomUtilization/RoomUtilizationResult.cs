namespace Application.Reports.RoomUtilization;

public sealed record RoomUtilizationResult(
    Guid ConferenceRoomId,
    string Name,
    int BookingsCount,
    decimal BookedHours);