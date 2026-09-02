using Application.Abstractions.Authentication;
using Application.Abstractions.Persistence;
using Application.Bookings.GetAll;
using Application.Common.Security;
using Domain.Entities;
using Moq;

namespace Application.Tests.Bookings.GetAll;

public sealed class GetBookingsHandlerTests
{
    private readonly Mock<IBookingRepository> _bookingRepository = new();
    private readonly Mock<ICurrentUser> _currentUser = new();

    private readonly GetBookingsHandler _handler;

    public GetBookingsHandlerTests()
    {
        _handler = new GetBookingsHandler(
            _bookingRepository.Object,
            _currentUser.Object);

        _currentUser
            .SetupGet(x => x.IsAuthenticated)
            .Returns(true);
    }

    [Fact]
    public async Task HandleAsync_Client_FiltersBookingsByCurrentUser()
    {
        var userId = Guid.NewGuid();

        _currentUser
            .SetupGet(x => x.UserId)
            .Returns(userId);

        _currentUser
            .SetupGet(x => x.Roles)
            .Returns(new[] { Roles.Client });

        _bookingRepository
            .Setup(x => x.GetAllAsync(
                userId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<Booking>());

        await _handler.HandleAsync();

        _bookingRepository.Verify(
            x => x.GetAllAsync(
                userId,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_Admin_DoesNotFilterByUser()
    {
        _currentUser
            .SetupGet(x => x.UserId)
            .Returns(Guid.NewGuid());

        _currentUser
            .SetupGet(x => x.Roles)
            .Returns(new[] { Roles.Admin });

        _bookingRepository
            .Setup(x => x.GetAllAsync(
                null,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<Booking>());

        await _handler.HandleAsync();

        _bookingRepository.Verify(
            x => x.GetAllAsync(
                null,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}