using Application.Abstractions.Persistence;
using Application.Common.Exceptions;

namespace Application.ConferenceRooms.Delete;

/// <summary>
/// Реалізує сценарій видалення конференц-залу.
/// Зал, який має пов'язані бронювання, видалити неможливо.
/// </summary>
public sealed class DeleteConferenceRoomHandler
{
    private readonly IConferenceRoomRepository _conferenceRoomRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteConferenceRoomHandler(
        IConferenceRoomRepository conferenceRoomRepository,
        IUnitOfWork unitOfWork)
    {
        _conferenceRoomRepository = conferenceRoomRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task HandleAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var conferenceRoom = await _conferenceRoomRepository.GetByIdAsync(
            id,
            cancellationToken);

        if (conferenceRoom is null)
        {
            throw new NotFoundException(
                $"Conference room with id '{id}' was not found.");
        }

        // Зал із бронюваннями не видаляємо, оскільки це призвело б
        // до втрати або порушення історичних даних про оренду.
        var hasBookings = await _conferenceRoomRepository.HasBookingsAsync(
            id,
            cancellationToken);

        if (hasBookings)
        {
            throw new ConflictException(
                "Conference room cannot be deleted because it has existing bookings.");
        }

        _conferenceRoomRepository.Remove(conferenceRoom);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}