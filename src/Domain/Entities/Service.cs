namespace Domain.Entities;

public class Service
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public decimal Price { get; set; }

    public ICollection<ConferenceRoom> ConferenceRooms { get; set; }
        = new List<ConferenceRoom>();

    public ICollection<Booking> Bookings { get; set; }
        = new List<Booking>();
}