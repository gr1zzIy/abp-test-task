using Application.Abstractions.Persistence;
using Application.Common.Exceptions;
using Domain.Entities;
using FluentValidation;

namespace Application.ConferenceRooms.Create;

public sealed class CreateConferenceRoomHandler
{
    private readonly IConferenceRoomRepository _conferenceRoomRepository;
    private readonly IServiceRepository _serviceRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IValidator<CreateConferenceRoomCommand> _validator;

    public CreateConferenceRoomHandler(
        IConferenceRoomRepository conferenceRoomRepository,
        IServiceRepository serviceRepository,
        IUnitOfWork unitOfWork,
        IValidator<CreateConferenceRoomCommand> validator)
    {
        _conferenceRoomRepository = conferenceRoomRepository;
        _serviceRepository = serviceRepository;
        _unitOfWork = unitOfWork;
        _validator = validator;
    }

    public async Task<CreateConferenceRoomResult> HandleAsync(
        CreateConferenceRoomCommand command,
        CancellationToken cancellationToken = default)
    {
        await _validator.ValidateAndThrowAsync(
            command,
            cancellationToken);
        
        var name = command.Name.Trim();

        var nameExists = await _conferenceRoomRepository.ExistsByNameAsync(
            name,
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

        var conferenceRoom = new ConferenceRoom
        {
            Id = Guid.NewGuid(),
            Name = name,
            Capacity = command.Capacity,
            HourlyRate = command.HourlyRate,
            Services = services
        };

        await _conferenceRoomRepository.AddAsync(
            conferenceRoom,
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new CreateConferenceRoomResult(conferenceRoom.Id);
    }
}