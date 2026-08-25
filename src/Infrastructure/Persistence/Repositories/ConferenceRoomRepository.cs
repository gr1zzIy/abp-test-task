using Application.Abstractions.Persistence;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

internal sealed class ConferenceRoomRepository : IConferenceRoomRepository
{
    private readonly AppDbContext _dbContext;

    public ConferenceRoomRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<bool> ExistsByNameAsync(
        string name,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.ConferenceRooms
            .AnyAsync(
                room => room.Name == name,
                cancellationToken);
    }

    public async Task AddAsync(
        ConferenceRoom conferenceRoom,
        CancellationToken cancellationToken = default)
    {
        await _dbContext.ConferenceRooms.AddAsync(
            conferenceRoom,
            cancellationToken);
    }
}