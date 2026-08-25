using Domain.Entities;

namespace Application.Abstractions.Persistence;

public interface IConferenceRoomRepository
{
    Task<bool> ExistsByNameAsync(
        string name,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        ConferenceRoom conferenceRoom,
        CancellationToken cancellationToken = default);
}