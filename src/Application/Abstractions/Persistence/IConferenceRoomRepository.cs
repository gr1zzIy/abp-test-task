using Domain.Entities;

namespace Application.Abstractions.Persistence;

/// <summary>
/// Надає операції доступу до даних конференц-залів.
/// </summary>
public interface IConferenceRoomRepository
{
    /// <summary>
    /// Перевіряє існування залу із заданою назвою.
    /// </summary>
    /// <param name="name">Назва залу.</param>
    /// <param name="excludeId">
    /// Ідентифікатор залу, який потрібно виключити з перевірки.
    /// Використовується під час редагування залу.
    /// </param>
    Task<bool> ExistsByNameAsync(
        string name,
        Guid? excludeId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Отримує всі конференц-зали.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<IReadOnlyCollection<ConferenceRoom>> GetAllAsync(
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Отримує конференц-зал за його ідентифікатором.
    /// </summary>
    /// <param name="id">Ідентифікатор залу.</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<ConferenceRoom?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Додає новий конференц-зал.
    /// </summary>
    /// <param name="conferenceRoom">Конференц-зал.</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task AddAsync(
        ConferenceRoom conferenceRoom,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Перевіряє, чи має конференц-зал бронювання.
    /// </summary>
    /// <param name="id">Ідентифікатор залу.</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<bool> HasBookingsAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Видаляє конференц-зал.
    /// </summary>
    /// <param name="conferenceRoom">Конференц-зал.</param>
    void Remove(ConferenceRoom conferenceRoom);
}