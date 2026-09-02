using Domain.Enums;

namespace Application.Bookings.GetAll;

public sealed record BookingListItemResult(
    Guid Id,
    Guid ConferenceRoomId,
    string ConferenceRoomName,
    DateTimeOffset StartTime,
    DateTimeOffset EndTime,
    decimal TotalPrice,
    BookingStatus Status);