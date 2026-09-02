using Domain.Enums;

namespace Domain.Entities;

/// <summary>
/// Представляє бронювання конференц-залу на визначений часовий проміжок.
/// </summary>
public class Booking
{
    public Guid Id { get; set; }

    /// <summary>
    /// Дата та час початку бронювання.
    /// </summary>
    public DateTimeOffset StartTime { get; set; }

    /// <summary>
    /// Дата та час завершення бронювання.
    /// </summary>
    public DateTimeOffset EndTime { get; set; }

    /// <summary>
    /// Остаточна вартість бронювання, зафіксована на момент його створення.
    /// Зберігається окремо, щоб подальша зміна тарифів не впливала
    /// на історичну вартість бронювання.
    /// </summary>
    public decimal TotalPrice { get; set; }

    /// <summary>
    /// Поточний стан бронювання.
    /// </summary>
    public BookingStatus Status { get; set; } = BookingStatus.Active;
    
    public Guid ConferenceRoomId { get; set; }
    public ConferenceRoom ConferenceRoom { get; set; } = null!;

    /// <summary>
    /// Ідентифікатор користувача, який створив бронювання.
    /// </summary>
    public Guid UserId { get; set; }
    
    /// <summary>
    /// Додаткові послуги, обрані для бронювання.
    /// </summary>
    public ICollection<Service> SelectedServices { get; set; }
        = new List<Service>();
}