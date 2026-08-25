using Domain.Entities;

namespace Application.Abstractions.Persistence;

public interface IConferenceRoomRepository
{
    Task<bool> ExistsByNameAsync(
        string name,
        Guid? excludeId = null,
        CancellationToken cancellationToken = default);

    Task<ConferenceRoom?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        ConferenceRoom conferenceRoom,
        CancellationToken cancellationToken = default);
}