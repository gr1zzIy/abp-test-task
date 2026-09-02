using Application.Abstractions.Persistence;
using Domain.Entities;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

internal sealed class BookingRepository : IBookingRepository
{
	private readonly AppDbContext _dbContext;

	public BookingRepository(AppDbContext dbContext)
	{
		_dbContext = dbContext;
	}

	public Task<bool> HasOverlapAsync(
			Guid conferenceRoomId,
			DateTimeOffset startTime,
			DateTimeOffset endTime,
			CancellationToken cancellationToken = default)
	{
		// Два часові проміжки перетинаються, якщо існуюче бронювання
		// починається до завершення нового і завершується після його початку.
		return _dbContext.Bookings.AnyAsync(
		booking =>
				booking.ConferenceRoomId == conferenceRoomId &&
				booking.Status == BookingStatus.Active &&
				booking.StartTime < endTime &&
				booking.EndTime > startTime,
		cancellationToken);
	}

	public async Task AddAsync(
			Booking booking,
			CancellationToken cancellationToken = default)
	{
		await _dbContext.Bookings.AddAsync(
		booking,
		cancellationToken);
	}
	
	public async Task<IReadOnlyCollection<Booking>> GetAllAsync(
		Guid? userId,
		CancellationToken cancellationToken = default)
	{
		var query = _dbContext.Bookings
			.AsNoTracking()
			.Include(booking => booking.ConferenceRoom)
			.Include(booking => booking.SelectedServices)
			.AsQueryable();

		if (userId.HasValue)
		{
			query = query.Where(
				booking => booking.UserId == userId.Value);
		}

		return await query
			.OrderByDescending(booking => booking.StartTime)
			.ToListAsync(cancellationToken);
	}
	
	public Task<Booking?> GetByIdAsync(
		Guid id,
		Guid? userId,
		CancellationToken cancellationToken = default)
	{
		var query = _dbContext.Bookings
			.Include(booking => booking.ConferenceRoom)
			.Include(booking => booking.SelectedServices)
			.AsQueryable();

		if (userId.HasValue)
		{
			query = query.Where(
				booking => booking.UserId == userId.Value);
		}

		return query.FirstOrDefaultAsync(
			booking => booking.Id == id,
			cancellationToken);
	}
}