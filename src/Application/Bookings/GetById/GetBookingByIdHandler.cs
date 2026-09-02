using Application.Abstractions.Authentication;
using Application.Abstractions.Persistence;
using Application.Common.Exceptions;
using Application.Common.Security;

namespace Application.Bookings.GetById;

/// <summary>
/// Повертає конкретне бронювання з урахуванням прав
/// поточного користувача.
/// </summary>
public sealed class GetBookingByIdHandler
{
    private readonly IBookingRepository _bookingRepository;
    private readonly ICurrentUser _currentUser;

    public GetBookingByIdHandler(
        IBookingRepository bookingRepository,
        ICurrentUser currentUser)
    {
        _bookingRepository = bookingRepository;
        _currentUser = currentUser;
    }

    public async Task<BookingDetailsResult> HandleAsync(
        Guid id,
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

        var booking = await _bookingRepository.GetByIdAsync(
            id,
            userId,
            cancellationToken);

        if (booking is null)
        {
            throw new NotFoundException(
                $"Booking with id '{id}' was not found.");
        }

        return new BookingDetailsResult(
            booking.Id,
            booking.ConferenceRoomId,
            booking.ConferenceRoom.Name,
            booking.StartTime,
            booking.EndTime,
            booking.TotalPrice,
            booking.Status,
            booking.SelectedServices
                .Select(service => new BookingServiceResult(
                    service.Id,
                    service.Name,
                    service.Price))
                .ToArray());
    }
}