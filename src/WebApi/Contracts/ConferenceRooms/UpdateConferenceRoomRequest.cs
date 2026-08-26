namespace WebApi.Contracts.ConferenceRooms;

/// <summary>
/// Дані для оновлення конференц-залу.
/// </summary>
/// <param name="Name">Нова назва залу.</param>
/// <param name="Capacity">Нова максимальна місткість.</param>
/// <param name="HourlyRate">Нова базова погодинна вартість.</param>
/// <param name="ServiceIds">
/// Повний список послуг, які повинні бути доступні для залу після оновлення.
/// </param>
public sealed record UpdateConferenceRoomRequest(
    string Name,
    int Capacity,
    decimal HourlyRate,
    IReadOnlyCollection<int> ServiceIds);