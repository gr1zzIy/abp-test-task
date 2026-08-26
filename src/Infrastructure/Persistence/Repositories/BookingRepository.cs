using Application.Abstractions.Persistence;
using Domain.Entities;
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
}