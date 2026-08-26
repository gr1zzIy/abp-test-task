using Application.Abstractions.Persistence;
using Application.Abstractions.Pricing;
using Application.Common.Exceptions;
using Domain.Entities;
using FluentValidation;

namespace Application.Bookings.Create;

/// <summary>
/// Реалізує сценарій бронювання конференц-залу:
/// перевіряє доступність залу та послуг, розраховує вартість
/// і створює бронювання.
/// </summary>
public sealed class CreateBookingHandler
{
    private readonly IConferenceRoomRepository _conferenceRoomRepository;
    private readonly IBookingRepository _bookingRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IRentalPriceCalculator _priceCalculator;
    private readonly IValidator<CreateBookingCommand> _validator;

    public CreateBookingHandler(
        IConferenceRoomRepository conferenceRoomRepository,
        IBookingRepository bookingRepository,
        IUnitOfWork unitOfWork,
        IRentalPriceCalculator priceCalculator,
        IValidator<CreateBookingCommand> validator)
    {
        _conferenceRoomRepository = conferenceRoomRepository;
        _bookingRepository = bookingRepository;
        _unitOfWork = unitOfWork;
        _priceCalculator = priceCalculator;
        _validator = validator;
    }

    public async Task<CreateBookingResult> HandleAsync(
        CreateBookingCommand command,
        CancellationToken cancellationToken = default)
    {
        await _validator.ValidateAndThrowAsync(
            command,
            cancellationToken);

        var conferenceRoom = await _conferenceRoomRepository.GetByIdAsync(
            command.ConferenceRoomId,
            cancellationToken);

        if (conferenceRoom is null)
        {
            throw new NotFoundException(
                $"Conference room with id '{command.ConferenceRoomId}' was not found.");
        }

        var serviceIds = command.ServiceIds
            .Distinct()
            .ToArray();

        var selectedServices = conferenceRoom.Services
            .Where(service => serviceIds.Contains(service.Id))
            .ToArray();

        if (selectedServices.Length != serviceIds.Length)
        {
            throw new BadRequestException(
                "One or more selected services are not available for this conference room.");
        }

        // перед перевіркою та збереженням нормалізуються до UTC.
        var startTimeUtc = command.StartTime.ToUniversalTime();
        var endTimeUtc = command.EndTime.ToUniversalTime();

        var hasOverlap = await _bookingRepository.HasOverlapAsync(
            conferenceRoom.Id,
            startTimeUtc,
            endTimeUtc,
            cancellationToken);

        if (hasOverlap)
        {
            throw new ConflictException(
                "Conference room is already booked for the selected time period.");
        }

        // Тарифні правила залежать від локального часу бронювання,
        // тому розрахунок виконуємо до перетворення часу в UTC.
        var rentalPrice = _priceCalculator.Calculate(
            conferenceRoom.HourlyRate,
            command.StartTime,
            command.EndTime);

        var servicesPrice = selectedServices.Sum(service => service.Price);

        var totalPrice = decimal.Round(
            rentalPrice + servicesPrice,
            2,
            MidpointRounding.AwayFromZero);

        var booking = new Booking
        {
            Id = Guid.NewGuid(),
            ConferenceRoomId = conferenceRoom.Id,
            StartTime = startTimeUtc,
            EndTime = endTimeUtc,
            TotalPrice = totalPrice,
            SelectedServices = selectedServices
        };

        await _bookingRepository.AddAsync(
            booking,
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new CreateBookingResult(
            booking.Id,
            booking.TotalPrice);
    }
}