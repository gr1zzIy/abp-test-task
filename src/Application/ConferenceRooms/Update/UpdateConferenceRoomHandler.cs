using Application.Abstractions.Persistence;
using Application.Common.Exceptions;
using FluentValidation;

namespace Application.ConferenceRooms.Update;

public sealed class UpdateConferenceRoomHandler
{
    private readonly IConferenceRoomRepository _conferenceRoomRepository;
    private readonly IServiceRepository _serviceRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IValidator<UpdateConferenceRoomCommand> _validator;

    public UpdateConferenceRoomHandler(
        IConferenceRoomRepository conferenceRoomRepository,
        IServiceRepository serviceRepository,
        IUnitOfWork unitOfWork,
        IValidator<UpdateConferenceRoomCommand> validator)
    {
        _conferenceRoomRepository = conferenceRoomRepository;
        _serviceRepository = serviceRepository;
        _unitOfWork = unitOfWork;
        _validator = validator;
    }

    public async Task HandleAsync(
        UpdateConferenceRoomCommand command,
        CancellationToken cancellationToken = default)
    {
        await _validator.ValidateAndThrowAsync(
            command,
            cancellationToken);

        var conferenceRoom = await _conferenceRoomRepository.GetByIdAsync(
            command.Id,
            cancellationToken);

        if (conferenceRoom is null)
        {
            throw new NotFoundException(
                $"Conference room with id '{command.Id}' was not found.");
        }

        var name = command.Name.Trim();

        var nameExists = await _conferenceRoomRepository.ExistsByNameAsync(
            name,
            command.Id,
            cancellationToken);

        if (nameExists)
        {
            throw new ConflictException(
                $"Conference room with name '{name}' already exists.");
        }

        var serviceIds = command.ServiceIds
            .Distinct()
            .ToArray();

        var services = await _serviceRepository.GetByIdsAsync(
            serviceIds,
            cancellationToken);

        if (services.Count != serviceIds.Length)
        {
            throw new BadRequestException(
                "One or more selected services do not exist.");
        }

        conferenceRoom.Name = name;
        conferenceRoom.Capacity = command.Capacity;
        conferenceRoom.HourlyRate = command.HourlyRate;

        conferenceRoom.Services.Clear();

        foreach (var service in services)
        {
            conferenceRoom.Services.Add(service);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}