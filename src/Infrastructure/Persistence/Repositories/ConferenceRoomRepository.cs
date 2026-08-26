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
    
    public async Task<IReadOnlyCollection<ConferenceRoom>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.ConferenceRooms
            .AsNoTracking()
            .Include(room => room.Services)
            .OrderBy(room => room.Name)
            .ToListAsync(cancellationToken);
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
    
    public Task<bool> HasBookingsAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.Bookings
            .AnyAsync(
                booking => booking.ConferenceRoomId == id,
                cancellationToken);
    }

    public void Remove(ConferenceRoom conferenceRoom)
    {
        _dbContext.ConferenceRooms.Remove(conferenceRoom);
    }
    
    public async Task<IReadOnlyCollection<ConferenceRoom>> GetAvailableAsync(
            DateTimeOffset startTime,
            DateTimeOffset endTime,
            int capacity,
            CancellationToken cancellationToken = default)
    {
        return await _dbContext.ConferenceRooms
                .AsNoTracking()
                .Include(room => room.Services)
                .Where(room => room.Capacity >= capacity)
                // Зал є доступним, якщо жодне існуюче бронювання
                // не перетинається із запитаним часовим проміжком.
                .Where(room => !room.Bookings.Any(booking =>
                        booking.StartTime < endTime &&
                        booking.EndTime > startTime))
                .OrderBy(room => room.Capacity)
                .ThenBy(room => room.Name)
                .ToListAsync(cancellationToken);
    }
}