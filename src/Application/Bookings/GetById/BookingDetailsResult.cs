using Domain.Enums;

namespace Application.Bookings.GetById;

public sealed record BookingDetailsResult(
    Guid Id,
    Guid ConferenceRoomId,
    string ConferenceRoomName,
    DateTimeOffset StartTime,
    DateTimeOffset EndTime,
    decimal TotalPrice,
    BookingStatus Status,
    IReadOnlyCollection<BookingServiceResult> Services);

public sealed record BookingServiceResult(
    int Id,
    string Name,
    decimal Price);