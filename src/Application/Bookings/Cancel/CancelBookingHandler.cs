using Application.Abstractions.Authentication;
using Application.Abstractions.Persistence;
using Application.Common.Exceptions;
using Application.Common.Security;
using Domain.Enums;

namespace Application.Bookings.Cancel;

/// <summary>
/// Скасовує бронювання без фізичного видалення історичних даних.
/// Клієнт може скасувати лише власне бронювання,
/// адміністратор будь-яке.
/// </summary>
public sealed class CancelBookingHandler
{
    private readonly IBookingRepository _bookingRepository;
    private readonly ICurrentUser _currentUser;
    private readonly IUnitOfWork _unitOfWork;
    private readonly TimeProvider _timeProvider;

    public CancelBookingHandler(
        IBookingRepository bookingRepository,
        ICurrentUser currentUser,
        IUnitOfWork unitOfWork,
        TimeProvider timeProvider)
    {
        _bookingRepository = bookingRepository;
        _currentUser = currentUser;
        _unitOfWork = unitOfWork;
        _timeProvider = timeProvider;
    }

    public async Task HandleAsync(
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

        if (booking.Status == BookingStatus.Cancelled)
        {
            throw new ConflictException(
                "Booking is already cancelled.");
        }

        // Завершене або вже розпочате бронювання не можна скасувати
        // через звичайний клієнтський сценарій.
        if (booking.StartTime <= _timeProvider.GetUtcNow())
        {
            throw new ConflictException(
                "Booking cannot be cancelled after it has started.");
        }

        booking.Status = BookingStatus.Cancelled;

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);
    }
}