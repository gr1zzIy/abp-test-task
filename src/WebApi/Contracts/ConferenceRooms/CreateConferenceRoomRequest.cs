namespace WebApi.Contracts.ConferenceRooms;

/// <summary>
/// Дані для створення конференц-залу.
/// </summary>
/// <param name="Name">Назва залу.</param>
/// <param name="Capacity">Максимальна місткість залу.</param>
/// <param name="HourlyRate">Базова вартість оренди за годину.</param>
/// <param name="ServiceIds">Ідентифікатори доступних додаткових послуг.</param>
public sealed record CreateConferenceRoomRequest(
    string Name,
    int Capacity,
    decimal HourlyRate,
    IReadOnlyCollection<int> ServiceIds);