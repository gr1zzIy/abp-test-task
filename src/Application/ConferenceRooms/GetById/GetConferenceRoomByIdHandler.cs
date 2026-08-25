using Application.Abstractions.Persistence;
using Application.Common.Exceptions;

namespace Application.ConferenceRooms.GetById;

public sealed class GetConferenceRoomByIdHandler
{
    private readonly IConferenceRoomRepository _conferenceRoomRepository;

    public GetConferenceRoomByIdHandler(
        IConferenceRoomRepository conferenceRoomRepository)
    {
        _conferenceRoomRepository = conferenceRoomRepository;
    }

    public async Task<GetConferenceRoomByIdResult> HandleAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var conferenceRoom = await _conferenceRoomRepository.GetByIdAsync(
            id,
            cancellationToken);

        if (conferenceRoom is null)
        {
            throw new NotFoundException(
                $"Conference room with id '{id}' was not found.");
        }

        return new GetConferenceRoomByIdResult(
            conferenceRoom.Id,
            conferenceRoom.Name,
            conferenceRoom.Capacity,
            conferenceRoom.HourlyRate,
            conferenceRoom.Services
                .Select(service => new ConferenceRoomServiceResult(
                    service.Id,
                    service.Name,
                    service.Price))
                .ToArray());
    }
}