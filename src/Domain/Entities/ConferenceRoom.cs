namespace Domain.Entities;

/// <summary>
/// Представляє конференц-зал, доступний для бронювання.
/// </summary>
public class ConferenceRoom
{
    public Guid Id { get; set; }

    /// <summary>
    /// Назва конференц-залу.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Максимальна кількість осіб, яку може вмістити зал.
    /// </summary>
    public int Capacity { get; set; }

    /// <summary>
    /// Базова вартість оренди залу за одну годину.
    /// </summary>
    public decimal HourlyRate { get; set; }

    /// <summary>
    /// Додаткові послуги, доступні для цього залу.
    /// </summary>
    public ICollection<Service> Services { get; set; }
        = new List<Service>();

    public ICollection<Booking> Bookings { get; set; }
        = new List<Booking>();
}