using Application.Abstractions.Authentication;
using Application.Abstractions.Persistence;
using Application.Common.Exceptions;
using Application.Common.Security;

namespace Application.Bookings.GetAll;

/// <summary>
/// Повертає бронювання, доступні поточному користувачу.
/// Адміністратор бачить усі бронювання, клієнт лише власні.
/// </summary>
public sealed class GetBookingsHandler
{
    private readonly IBookingRepository _bookingRepository;
    private readonly ICurrentUser _currentUser;

    public GetBookingsHandler(
        IBookingRepository bookingRepository,
        ICurrentUser currentUser)
    {
        _bookingRepository = bookingRepository;
        _currentUser = currentUser;
    }

    public async Task<IReadOnlyCollection<BookingListItemResult>> HandleAsync(
        CancellationToken cancellationToken = default)
    {
        if (!_currentUser.IsAuthenticated ||
            _currentUser.UserId is null)
        {
            throw new UnauthorizedException(
                "User is not authenticated.");
        }

        var isAdmin = _currentUser.Roles.Contains(Roles.Admin);

        var userId = isAdmin
            ? (Guid?)null
            : _currentUser.UserId.Value;

        var bookings = await _bookingRepository.GetAllAsync(
            userId,
            cancellationToken);

        return bookings
            .Select(booking => new BookingListItemResult(
                booking.Id,
                booking.ConferenceRoomId,
                booking.ConferenceRoom.Name,
                booking.StartTime,
                booking.EndTime,
                booking.TotalPrice,
                booking.Status))
            .ToArray();
    }
}