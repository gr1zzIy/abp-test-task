namespace Domain.Entities;

/// <summary>
/// Представляє додаткову послугу, яку можна використати під час оренди залу.
/// </summary>
public class Service
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Вартість послуги.
    /// </summary>
    public decimal Price { get; set; }

    public ICollection<ConferenceRoom> ConferenceRooms { get; set; }
        = new List<ConferenceRoom>();

    public ICollection<Booking> Bookings { get; set; }
        = new List<Booking>();
}