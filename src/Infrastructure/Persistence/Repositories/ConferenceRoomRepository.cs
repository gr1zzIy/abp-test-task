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
        Guid? excludeId = null,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.ConferenceRooms
            .AnyAsync(
                room =>
                    room.Name == name &&
                    (!excludeId.HasValue || room.Id != excludeId.Value),
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
    
    public Task<ConferenceRoom?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.ConferenceRooms
            .Include(room => room.Services)
            .FirstOrDefaultAsync(
                room => room.Id == id,
                cancellationToken);
    }
}