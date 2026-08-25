using Application.Abstractions.Persistence;

namespace Application.ConferenceRooms.GetAll;

public sealed class GetConferenceRoomsHandler
{
    private readonly IConferenceRoomRepository _conferenceRoomRepository;

    public GetConferenceRoomsHandler(
        IConferenceRoomRepository conferenceRoomRepository)
    {
        _conferenceRoomRepository = conferenceRoomRepository;
    }

    public async Task<IReadOnlyCollection<GetConferenceRoomsResult>> HandleAsync(
        CancellationToken cancellationToken = default)
    {
        var conferenceRooms =
            await _conferenceRoomRepository.GetAllAsync(cancellationToken);

        return conferenceRooms
            .Select(room => new GetConferenceRoomsResult(
                room.Id,
                room.Name,
                room.Capacity,
                room.HourlyRate,
                room.Services
                    .Select(service => new ConferenceRoomServiceResult(
                        service.Id,
                        service.Name,
                        service.Price))
                    .ToArray()))
            .ToArray();
    }
}