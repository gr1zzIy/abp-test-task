using Application.Abstractions.Authentication;
using Application.Abstractions.Persistence;
using Application.Bookings.Cancel;
using Application.Common.Exceptions;
using Application.Common.Security;
using Application.Tests.Common;
using Domain.Entities;
using Domain.Enums;
using Moq;

namespace Application.Tests.Bookings.Cancel;

public sealed class CancelBookingHandlerTests
{
    private static readonly DateTimeOffset Now =
        new(
            2026, 9, 2,
            10, 0, 0,
            TimeSpan.Zero);

    private readonly Mock<IBookingRepository> _bookingRepository = new();
    private readonly Mock<ICurrentUser> _currentUser = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();

    private readonly TestTimeProvider _timeProvider;

    public CancelBookingHandlerTests()
    {
        _timeProvider = new TestTimeProvider(Now);

        _currentUser
            .SetupGet(x => x.IsAuthenticated)
            .Returns(true);
    }

    [Fact]
    public async Task HandleAsync_ActiveFutureBooking_CancelsBooking()
    {
        var userId = Guid.NewGuid();

        var booking = CreateBooking(
            userId,
            startTime: Now.AddHours(2),
            status: BookingStatus.Active);

        ConfigureClient(userId);

        _bookingRepository
            .Setup(x => x.GetByIdAsync(
                booking.Id,
                userId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(booking);

        var handler = CreateHandler();

        await handler.HandleAsync(
            booking.Id);

        Assert.Equal(
            BookingStatus.Cancelled,
            booking.Status);

        _unitOfWork.Verify(
            x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_AlreadyCancelledBooking_ThrowsConflictException()
    {
        var userId = Guid.NewGuid();

        var booking = CreateBooking(
            userId,
            startTime: Now.AddHours(2),
            status: BookingStatus.Cancelled);

        ConfigureClient(userId);

        _bookingRepository
            .Setup(x => x.GetByIdAsync(
                booking.Id,
                userId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(booking);

        var handler = CreateHandler();

        await Assert.ThrowsAsync<ConflictException>(() =>
            handler.HandleAsync(booking.Id));

        _unitOfWork.Verify(
            x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task HandleAsync_StartedBooking_ThrowsConflictException()
    {
        var userId = Guid.NewGuid();

        var booking = CreateBooking(
            userId,
            startTime: Now.AddMinutes(-30),
            status: BookingStatus.Active);

        ConfigureClient(userId);

        _bookingRepository
            .Setup(x => x.GetByIdAsync(
                booking.Id,
                userId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(booking);

        var handler = CreateHandler();

        await Assert.ThrowsAsync<ConflictException>(() =>
            handler.HandleAsync(booking.Id));

        Assert.Equal(
            BookingStatus.Active,
            booking.Status);

        _unitOfWork.Verify(
            x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task HandleAsync_BookingStartsNow_ThrowsConflictException()
    {
        var userId = Guid.NewGuid();

        var booking = CreateBooking(
            userId,
            startTime: Now,
            status: BookingStatus.Active);

        ConfigureClient(userId);

        _bookingRepository
            .Setup(x => x.GetByIdAsync(
                booking.Id,
                userId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(booking);

        var handler = CreateHandler();

        await Assert.ThrowsAsync<ConflictException>(() =>
            handler.HandleAsync(booking.Id));

        _unitOfWork.Verify(
            x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task HandleAsync_BookingNotOwnedByClient_ThrowsNotFoundException()
    {
        var userId = Guid.NewGuid();
        var bookingId = Guid.NewGuid();

        ConfigureClient(userId);

        _bookingRepository
            .Setup(x => x.GetByIdAsync(
                bookingId,
                userId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Booking?)null);

        var handler = CreateHandler();

        await Assert.ThrowsAsync<NotFoundException>(() =>
            handler.HandleAsync(bookingId));

        _unitOfWork.Verify(
            x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task HandleAsync_Admin_CanCancelAnyBooking()
    {
        var adminId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();

        var booking = CreateBooking(
            ownerId,
            startTime: Now.AddHours(2),
            status: BookingStatus.Active);

        ConfigureAdmin(adminId);

        _bookingRepository
            .Setup(x => x.GetByIdAsync(
                booking.Id,
                null,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(booking);

        var handler = CreateHandler();

        await handler.HandleAsync(
            booking.Id);

        Assert.Equal(
            BookingStatus.Cancelled,
            booking.Status);

        _bookingRepository.Verify(
            x => x.GetByIdAsync(
                booking.Id,
                null,
                It.IsAny<CancellationToken>()),
            Times.Once);

        _unitOfWork.Verify(
            x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_UnauthenticatedUser_ThrowsUnauthorizedException()
    {
        _currentUser
            .SetupGet(x => x.IsAuthenticated)
            .Returns(false);

        _currentUser
            .SetupGet(x => x.UserId)
            .Returns((Guid?)null);

        var handler = CreateHandler();

        await Assert.ThrowsAsync<UnauthorizedException>(() =>
            handler.HandleAsync(Guid.NewGuid()));

        _bookingRepository.Verify(
            x => x.GetByIdAsync(
                It.IsAny<Guid>(),
                It.IsAny<Guid?>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

        _unitOfWork.Verify(
            x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task HandleAsync_AuthenticatedUserWithoutUserId_ThrowsUnauthorizedException()
    {
        _currentUser
            .SetupGet(x => x.IsAuthenticated)
            .Returns(true);

        _currentUser
            .SetupGet(x => x.UserId)
            .Returns((Guid?)null);

        var handler = CreateHandler();

        await Assert.ThrowsAsync<UnauthorizedException>(() =>
            handler.HandleAsync(Guid.NewGuid()));

        _bookingRepository.Verify(
            x => x.GetByIdAsync(
                It.IsAny<Guid>(),
                It.IsAny<Guid?>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    private CancelBookingHandler CreateHandler()
    {
        return new CancelBookingHandler(
            _bookingRepository.Object,
            _currentUser.Object,
            _unitOfWork.Object,
            _timeProvider);
    }

    private void ConfigureClient(Guid userId)
    {
        _currentUser
            .SetupGet(x => x.UserId)
            .Returns(userId);

        _currentUser
            .SetupGet(x => x.Roles)
            .Returns(new[] { Roles.Client });
    }

    private void ConfigureAdmin(Guid userId)
    {
        _currentUser
            .SetupGet(x => x.UserId)
            .Returns(userId);

        _currentUser
            .SetupGet(x => x.Roles)
            .Returns(new[] { Roles.Admin });
    }

    private static Booking CreateBooking(
        Guid userId,
        DateTimeOffset startTime,
        BookingStatus status)
    {
        return new Booking
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            ConferenceRoomId = Guid.NewGuid(),
            StartTime = startTime,
            EndTime = startTime.AddHours(2),
            TotalPrice = 4000m,
            Status = status
        };
    }
}