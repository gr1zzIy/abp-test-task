using Domain.Entities;

namespace Application.Abstractions.Persistence;

/// <summary>
/// Надає операції доступу до даних бронювань.
/// </summary>
public interface IBookingRepository
{
	Task<bool> HasOverlapAsync(
			Guid conferenceRoomId,
			DateTimeOffset startTime,
			DateTimeOffset endTime,
			CancellationToken cancellationToken = default);

	Task AddAsync(
			Booking booking,
			CancellationToken cancellationToken = default);
	
	Task<IReadOnlyCollection<Booking>> GetAllAsync(
		Guid? userId,
		CancellationToken cancellationToken = default);

	Task<Booking?> GetByIdAsync(
		Guid id,
		Guid? userId,
		CancellationToken cancellationToken = default);
}