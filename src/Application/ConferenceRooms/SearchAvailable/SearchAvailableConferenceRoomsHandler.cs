using Application.Abstractions.Persistence;
using FluentValidation;

namespace Application.ConferenceRooms.SearchAvailable;

/// <summary>
/// Виконує пошук конференц-залів, які відповідають вимогам
/// щодо місткості та не мають бронювань у заданий період.
/// </summary>
public sealed class SearchAvailableConferenceRoomsHandler
{
	private readonly IConferenceRoomRepository _conferenceRoomRepository;
	private readonly IValidator<SearchAvailableConferenceRoomsQuery> _validator;

	public SearchAvailableConferenceRoomsHandler(
			IConferenceRoomRepository conferenceRoomRepository,
			IValidator<SearchAvailableConferenceRoomsQuery> validator)
	{
		_conferenceRoomRepository = conferenceRoomRepository;
		_validator = validator;
	}

	public async Task<IReadOnlyCollection<AvailableConferenceRoomResult>> HandleAsync(
			SearchAvailableConferenceRoomsQuery query,
			CancellationToken cancellationToken = default)
	{
		await _validator.ValidateAndThrowAsync(
		query,
		cancellationToken);

		// перед зверненням до БД нормалізуємо часовий проміжок до UTC.
		var startTimeUtc = query.StartTime.ToUniversalTime();
		var endTimeUtc = query.EndTime.ToUniversalTime();

		var rooms = await _conferenceRoomRepository.GetAvailableAsync(
		startTimeUtc,
		endTimeUtc,
		query.Capacity,
		cancellationToken);

		return rooms
				.Select(room => new AvailableConferenceRoomResult(
				room.Id,
				room.Name,
				room.Capacity,
				room.HourlyRate,
				room.Services
						.Select(service => new AvailableConferenceRoomServiceResult(
						service.Id,
						service.Name,
						service.Price))
						.ToArray()))
				.ToArray();
	}
}