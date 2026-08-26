namespace WebApi.Contracts.Bookings;

/// <summary>
/// Дані для створення бронювання конференц-залу.
/// </summary>
/// <param name="ConferenceRoomId">Ідентифікатор залу.</param>
/// <param name="StartTime">Дата та час початку бронювання.</param>
/// <param name="EndTime">Дата та час завершення бронювання.</param>
/// <param name="ServiceIds">Ідентифікатори вибраних додаткових послуг.</param>
public sealed record CreateBookingRequest(
    Guid ConferenceRoomId,
    DateTimeOffset StartTime,
    DateTimeOffset EndTime,
    IReadOnlyCollection<int> ServiceIds);