using Application.Abstractions.Authentication;
using Application.Abstractions.Persistence;
using Application.Bookings.GetById;
using Application.Common.Exceptions;
using Application.Common.Security;
using Domain.Entities;
using Domain.Enums;
using Moq;

namespace Application.Tests.Bookings.GetById;

public sealed class GetBookingByIdHandlerTests
{
    private readonly Mock<IBookingRepository> _bookingRepository = new();
    private readonly Mock<ICurrentUser> _currentUser = new();

    private readonly GetBookingByIdHandler _handler;

    public GetBookingByIdHandlerTests()
    {
        _handler = new GetBookingByIdHandler(
            _bookingRepository.Object,
            _currentUser.Object);

        _currentUser
            .SetupGet(x => x.IsAuthenticated)
            .Returns(true);
    }

    [Fact]
    public async Task HandleAsync_Client_FiltersBookingByCurrentUser()
    {
        var userId = Guid.NewGuid();
        var bookingId = Guid.NewGuid();

        var booking = CreateBooking(
            bookingId,
            userId);

        _currentUser
            .SetupGet(x => x.UserId)
            .Returns(userId);

        _currentUser
            .SetupGet(x => x.Roles)
            .Returns(new[] { Roles.Client });

        _bookingRepository
            .Setup(x => x.GetByIdAsync(
                bookingId,
                userId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(booking);

        var result = await _handler.HandleAsync(
            bookingId);

        Assert.Equal(bookingId, result.Id);

        _bookingRepository.Verify(
            x => x.GetByIdAsync(
                bookingId,
                userId,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_Admin_DoesNotFilterBookingByUser()
    {
        var bookingId = Guid.NewGuid();

        var booking = CreateBooking(
            bookingId,
            Guid.NewGuid());

        _currentUser
            .SetupGet(x => x.UserId)
            .Returns(Guid.NewGuid());

        _currentUser
            .SetupGet(x => x.Roles)
            .Returns(new[] { Roles.Admin });

        _bookingRepository
            .Setup(x => x.GetByIdAsync(
                bookingId,
                null,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(booking);

        var result = await _handler.HandleAsync(
            bookingId);

        Assert.Equal(bookingId, result.Id);

        _bookingRepository.Verify(
            x => x.GetByIdAsync(
                bookingId,
                null,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_BookingNotAccessibleToClient_ThrowsNotFoundException()
    {
        var userId = Guid.NewGuid();
        var bookingId = Guid.NewGuid();

        _currentUser
            .SetupGet(x => x.UserId)
            .Returns(userId);

        _currentUser
            .SetupGet(x => x.Roles)
            .Returns(new[] { Roles.Client });

        _bookingRepository
            .Setup(x => x.GetByIdAsync(
                bookingId,
                userId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Booking?)null);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            _handler.HandleAsync(bookingId));
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

        await Assert.ThrowsAsync<UnauthorizedException>(() =>
            _handler.HandleAsync(Guid.NewGuid()));

        _bookingRepository.Verify(
            x => x.GetByIdAsync(
                It.IsAny<Guid>(),
                It.IsAny<Guid?>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task HandleAsync_ExistingBooking_ReturnsMappedDetails()
    {
        var userId = Guid.NewGuid();
        var bookingId = Guid.NewGuid();
        var roomId = Guid.NewGuid();

        var projector = new Service
        {
            Id = 1,
            Name = "Проєктор",
            Price = 500m
        };

        var booking = new Booking
        {
            Id = bookingId,
            UserId = userId,
            ConferenceRoomId = roomId,
            ConferenceRoom = new ConferenceRoom
            {
                Id = roomId,
                Name = "Зал A",
                Capacity = 50,
                HourlyRate = 2000m
            },
            StartTime = new DateTimeOffset(
                2026, 9, 10,
                9, 0, 0,
                TimeSpan.Zero),
            EndTime = new DateTimeOffset(
                2026, 9, 10,
                11, 0, 0,
                TimeSpan.Zero),
            TotalPrice = 4500m,
            Status = BookingStatus.Active,
            SelectedServices = new List<Service>
            {
                projector
            }
        };

        _currentUser
            .SetupGet(x => x.UserId)
            .Returns(userId);

        _currentUser
            .SetupGet(x => x.Roles)
            .Returns(new[] { Roles.Client });

        _bookingRepository
            .Setup(x => x.GetByIdAsync(
                bookingId,
                userId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(booking);

        var result = await _handler.HandleAsync(
            bookingId);

        Assert.Equal(booking.Id, result.Id);
        Assert.Equal(
            booking.ConferenceRoomId,
            result.ConferenceRoomId);

        Assert.Equal(
            "Зал A",
            result.ConferenceRoomName);

        Assert.Equal(
            booking.StartTime,
            result.StartTime);

        Assert.Equal(
            booking.EndTime,
            result.EndTime);

        Assert.Equal(
            booking.TotalPrice,
            result.TotalPrice);

        Assert.Equal(
            BookingStatus.Active,
            result.Status);

        var service = Assert.Single(
            result.Services);

        Assert.Equal(projector.Id, service.Id);
        Assert.Equal(projector.Name, service.Name);
        Assert.Equal(projector.Price, service.Price);
    }

    private static Booking CreateBooking(
        Guid bookingId,
        Guid userId)
    {
        var roomId = Guid.NewGuid();

        return new Booking
        {
            Id = bookingId,
            UserId = userId,
            ConferenceRoomId = roomId,
            ConferenceRoom = new ConferenceRoom
            {
                Id = roomId,
                Name = "Зал A",
                Capacity = 50,
                HourlyRate = 2000m
            },
            StartTime = new DateTimeOffset(
                2026, 9, 10,
                9, 0, 0,
                TimeSpan.Zero),
            EndTime = new DateTimeOffset(
                2026, 9, 10,
                11, 0, 0,
                TimeSpan.Zero),
            TotalPrice = 4000m,
            Status = BookingStatus.Active
        };
    }
}