using Application.Abstractions.Persistence;
using Application.ConferenceRooms.SearchAvailable;
using Domain.Entities;
using FluentValidation;
using FluentValidation.Results;
using Moq;

namespace Application.Tests.ConferenceRooms.SearchAvailable;

public sealed class SearchAvailableConferenceRoomsHandlerTests
{
    private readonly Mock<IConferenceRoomRepository> _repository = new();
    private readonly Mock<IValidator<SearchAvailableConferenceRoomsQuery>> _validator = new();

    private readonly SearchAvailableConferenceRoomsHandler _handler;

    public SearchAvailableConferenceRoomsHandlerTests()
    {
        _handler = new SearchAvailableConferenceRoomsHandler(
            _repository.Object,
            _validator.Object);

        _validator
            .Setup(x => x.ValidateAsync(
                It.IsAny<SearchAvailableConferenceRoomsQuery>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());
    }

    [Fact]
    public async Task HandleAsync_AvailableRooms_ReturnsMappedResults()
    {
        var projector = new Service
        {
            Id = 1,
            Name = "Проєктор",
            Price = 500m
        };

        var room = new ConferenceRoom
        {
            Id = Guid.NewGuid(),
            Name = "Зал A",
            Capacity = 50,
            HourlyRate = 2000m,
            Services = [projector]
        };

        var query = CreateQuery();

        _repository
            .Setup(x => x.GetAvailableAsync(
                query.StartTime.ToUniversalTime(),
                query.EndTime.ToUniversalTime(),
                query.Capacity,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync([room]);

        var result = await _handler.HandleAsync(query);

        var returnedRoom = Assert.Single(result);

        Assert.Equal(room.Id, returnedRoom.Id);
        Assert.Equal("Зал A", returnedRoom.Name);
        Assert.Equal(50, returnedRoom.Capacity);
        Assert.Equal(2000m, returnedRoom.HourlyRate);

        var returnedService = Assert.Single(returnedRoom.Services);

        Assert.Equal(projector.Id, returnedService.Id);
        Assert.Equal(projector.Name, returnedService.Name);
        Assert.Equal(projector.Price, returnedService.Price);
    }

    [Fact]
    public async Task HandleAsync_NoAvailableRooms_ReturnsEmptyCollection()
    {
        var query = CreateQuery();

        _repository
            .Setup(x => x.GetAvailableAsync(
                It.IsAny<DateTimeOffset>(),
                It.IsAny<DateTimeOffset>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var result = await _handler.HandleAsync(query);

        Assert.Empty(result);
    }

    [Fact]
    public async Task HandleAsync_SearchPeriod_IsConvertedToUtc()
    {
        var query = CreateQuery();

        _repository
            .Setup(x => x.GetAvailableAsync(
                It.IsAny<DateTimeOffset>(),
                It.IsAny<DateTimeOffset>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        await _handler.HandleAsync(query);

        // PostgreSQL зберігає час бронювань у UTC, тому й пошук
        // доступності повинен виконуватись у тій самій часовій системі.
        _repository.Verify(
            x => x.GetAvailableAsync(
                query.StartTime.ToUniversalTime(),
                query.EndTime.ToUniversalTime(),
                query.Capacity,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    private static SearchAvailableConferenceRoomsQuery CreateQuery()
    {
        return new SearchAvailableConferenceRoomsQuery(
            new DateTimeOffset(
                2026, 9, 1, 10, 0, 0,
                TimeSpan.FromHours(3)),
            new DateTimeOffset(
                2026, 9, 1, 14, 0, 0,
                TimeSpan.FromHours(3)),
            50);
    }
}