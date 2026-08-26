using Application.Abstractions.Persistence;
using Application.Abstractions.Pricing;
using Application.Abstractions.Time;
using Application.Bookings.Create;
using Application.Common.Exceptions;
using Domain.Entities;
using FluentValidation;
using FluentValidation.Results;
using Moq;

namespace Application.Tests.Bookings.Create;

public sealed class CreateBookingHandlerTests
{
    private readonly Mock<IConferenceRoomRepository> _conferenceRoomRepository = new();
    private readonly Mock<IBookingRepository> _bookingRepository = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly Mock<IRentalPriceCalculator> _priceCalculator = new();
    private readonly Mock<IValidator<CreateBookingCommand>> _validator = new();

    private readonly CreateBookingHandler _handler;
    private readonly Mock<IBusinessTimeZone> _businessTimeZone = new();
    
    public CreateBookingHandlerTests()
    {
        _handler = new CreateBookingHandler(
            _conferenceRoomRepository.Object,
            _bookingRepository.Object,
            _unitOfWork.Object,
            _priceCalculator.Object,
            _businessTimeZone.Object,
            _validator.Object);

        _validator
            .Setup(x => x.ValidateAsync(
                It.IsAny<CreateBookingCommand>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());
        
        _businessTimeZone
            .Setup(x => x.ConvertFromUtc(It.IsAny<DateTimeOffset>()))
            .Returns((DateTimeOffset utcTime) =>
                utcTime.ToOffset(TimeSpan.FromHours(3)));
    }

    [Fact]
    public async Task HandleAsync_ConferenceRoomDoesNotExist_ThrowsNotFoundException()
    {
        var command = CreateCommand();

        _conferenceRoomRepository
            .Setup(x => x.GetByIdAsync(
                command.ConferenceRoomId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((ConferenceRoom?)null);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            _handler.HandleAsync(command));
    }

    [Fact]
    public async Task HandleAsync_ServiceIsNotAvailableForRoom_ThrowsBadRequestException()
    {
        var room = CreateConferenceRoom();

        var command = CreateCommand(
            conferenceRoomId: room.Id,
            serviceIds: [999]);

        _conferenceRoomRepository
            .Setup(x => x.GetByIdAsync(
                room.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(room);

        await Assert.ThrowsAsync<BadRequestException>(() =>
            _handler.HandleAsync(command));
    }

    [Fact]
    public async Task HandleAsync_TimePeriodOverlapsExistingBooking_ThrowsConflictException()
    {
        var room = CreateConferenceRoom();
        var command = CreateCommand(conferenceRoomId: room.Id);

        _conferenceRoomRepository
            .Setup(x => x.GetByIdAsync(
                room.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(room);

        _bookingRepository
            .Setup(x => x.HasOverlapAsync(
                room.Id,
                command.StartTime.ToUniversalTime(),
                command.EndTime.ToUniversalTime(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        await Assert.ThrowsAsync<ConflictException>(() =>
            _handler.HandleAsync(command));

        _bookingRepository.Verify(
            x => x.AddAsync(
                It.IsAny<Booking>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

        _unitOfWork.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task HandleAsync_ValidCommand_CreatesBooking()
    {
        var room = CreateConferenceRoom();
        var command = CreateCommand(conferenceRoomId: room.Id);

        SetupAvailableRoom(room, command);

        _priceCalculator
            .Setup(x => x.Calculate(
                room.HourlyRate,
                command.StartTime,
                command.EndTime))
            .Returns(4000m);

        Booking? createdBooking = null;

        _bookingRepository
            .Setup(x => x.AddAsync(
                It.IsAny<Booking>(),
                It.IsAny<CancellationToken>()))
            .Callback<Booking, CancellationToken>(
                (booking, _) => createdBooking = booking)
            .Returns(Task.CompletedTask);

        _unitOfWork
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var result = await _handler.HandleAsync(command);

        Assert.NotNull(createdBooking);
        Assert.Equal(room.Id, createdBooking.ConferenceRoomId);
        Assert.Equal(4000m, createdBooking.TotalPrice);
        Assert.Equal(createdBooking.Id, result.Id);
        Assert.Equal(createdBooking.TotalPrice, result.TotalPrice);

        _bookingRepository.Verify(
            x => x.AddAsync(
                It.IsAny<Booking>(),
                It.IsAny<CancellationToken>()),
            Times.Once);

        _unitOfWork.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_ValidCommand_StoresBookingTimeInUtc()
    {
        var room = CreateConferenceRoom();
        var command = CreateCommand(conferenceRoomId: room.Id);

        SetupAvailableRoom(room, command);

        _priceCalculator
            .Setup(x => x.Calculate(
                It.IsAny<decimal>(),
                It.IsAny<DateTimeOffset>(),
                It.IsAny<DateTimeOffset>()))
            .Returns(4000m);

        Booking? createdBooking = null;

        _bookingRepository
            .Setup(x => x.AddAsync(
                It.IsAny<Booking>(),
                It.IsAny<CancellationToken>()))
            .Callback<Booking, CancellationToken>(
                (booking, _) => createdBooking = booking)
            .Returns(Task.CompletedTask);

        await _handler.HandleAsync(command);

        Assert.NotNull(createdBooking);

        Assert.Equal(
            TimeSpan.Zero,
            createdBooking.StartTime.Offset);

        Assert.Equal(
            TimeSpan.Zero,
            createdBooking.EndTime.Offset);

        Assert.Equal(
            command.StartTime.ToUniversalTime(),
            createdBooking.StartTime);

        Assert.Equal(
            command.EndTime.ToUniversalTime(),
            createdBooking.EndTime);
    }

    [Fact]
    public async Task HandleAsync_SelectedServices_AddsServicePricesToTotal()
    {
        var projector = new Service
        {
            Id = 1,
            Name = "Проєктор",
            Price = 500m
        };

        var wifi = new Service
        {
            Id = 2,
            Name = "Wi-Fi",
            Price = 300m
        };

        var room = CreateConferenceRoom(
            services: [projector, wifi]);

        var command = CreateCommand(
            conferenceRoomId: room.Id,
            serviceIds: [1, 2]);

        SetupAvailableRoom(room, command);

        _priceCalculator
            .Setup(x => x.Calculate(
                room.HourlyRate,
                command.StartTime,
                command.EndTime))
            .Returns(4000m);

        Booking? createdBooking = null;

        _bookingRepository
            .Setup(x => x.AddAsync(
                It.IsAny<Booking>(),
                It.IsAny<CancellationToken>()))
            .Callback<Booking, CancellationToken>(
                (booking, _) => createdBooking = booking)
            .Returns(Task.CompletedTask);

        var result = await _handler.HandleAsync(command);

        Assert.Equal(4800m, result.TotalPrice);

        Assert.NotNull(createdBooking);
        Assert.Equal(2, createdBooking.SelectedServices.Count);
    }

    [Fact]
    public async Task HandleAsync_DuplicateServiceIds_DoesNotChargeServiceTwice()
    {
        var projector = new Service
        {
            Id = 1,
            Name = "Проєктор",
            Price = 500m
        };

        var room = CreateConferenceRoom(
            services: [projector]);

        var command = CreateCommand(
            conferenceRoomId: room.Id,
            serviceIds: [1, 1, 1]);

        SetupAvailableRoom(room, command);

        _priceCalculator
            .Setup(x => x.Calculate(
                It.IsAny<decimal>(),
                It.IsAny<DateTimeOffset>(),
                It.IsAny<DateTimeOffset>()))
            .Returns(4000m);

        _bookingRepository
            .Setup(x => x.AddAsync(
                It.IsAny<Booking>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await _handler.HandleAsync(command);

        Assert.Equal(4500m, result.TotalPrice);
    }

    private void SetupAvailableRoom(
        ConferenceRoom room,
        CreateBookingCommand command)
    {
        _conferenceRoomRepository
            .Setup(x => x.GetByIdAsync(
                room.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(room);

        _bookingRepository
            .Setup(x => x.HasOverlapAsync(
                room.Id,
                command.StartTime.ToUniversalTime(),
                command.EndTime.ToUniversalTime(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _unitOfWork
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);
    }

    [Fact]
    public async Task HandleAsync_RequestWithDifferentOffset_UsesBusinessTimeForPricing()
    {
        var room = CreateConferenceRoom();

        var command = new CreateBookingCommand(
            room.Id,
            new DateTimeOffset(
                2026, 9, 1, 9, 0, 0,
                TimeSpan.Zero),
            new DateTimeOffset(
                2026, 9, 1, 11, 0, 0,
                TimeSpan.Zero),
            []);

        _conferenceRoomRepository
            .Setup(x => x.GetByIdAsync(
                room.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(room);

        _businessTimeZone
            .Setup(x => x.ConvertFromUtc(command.StartTime.ToUniversalTime()))
            .Returns(new DateTimeOffset(
                2026, 9, 1, 12, 0, 0,
                TimeSpan.FromHours(3)));

        _businessTimeZone
            .Setup(x => x.ConvertFromUtc(command.EndTime.ToUniversalTime()))
            .Returns(new DateTimeOffset(
                2026, 9, 1, 14, 0, 0,
                TimeSpan.FromHours(3)));

        _bookingRepository
            .Setup(x => x.HasOverlapAsync(
                room.Id,
                command.StartTime.ToUniversalTime(),
                command.EndTime.ToUniversalTime(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _priceCalculator
            .Setup(x => x.Calculate(
                room.HourlyRate,
                It.IsAny<DateTimeOffset>(),
                It.IsAny<DateTimeOffset>()))
            .Returns(4600m);

        await _handler.HandleAsync(command);

        // Незалежно від offset клієнта тарифікація виконується
        // за локальним бізнесовим часом 12:00–14:00.
        _priceCalculator.Verify(
            x => x.Calculate(
                room.HourlyRate,
                new DateTimeOffset(
                    2026, 9, 1, 12, 0, 0,
                    TimeSpan.FromHours(3)),
                new DateTimeOffset(
                    2026, 9, 1, 14, 0, 0,
                    TimeSpan.FromHours(3))),
            Times.Once);
    }
    
    private static ConferenceRoom CreateConferenceRoom(
        ICollection<Service>? services = null)
    {
        return new ConferenceRoom
        {
            Id = Guid.NewGuid(),
            Name = "Зал A",
            Capacity = 50,
            HourlyRate = 2000m,
            Services = services ?? new List<Service>()
        };
    }

    private static CreateBookingCommand CreateCommand(
        Guid? conferenceRoomId = null,
        IReadOnlyCollection<int>? serviceIds = null)
    {
        return new CreateBookingCommand(
            conferenceRoomId ?? Guid.NewGuid(),
            new DateTimeOffset(
                2026, 9, 1, 10, 0, 0,
                TimeSpan.FromHours(3)),
            new DateTimeOffset(
                2026, 9, 1, 12, 0, 0,
                TimeSpan.FromHours(3)),
            serviceIds ?? []);
    }
}