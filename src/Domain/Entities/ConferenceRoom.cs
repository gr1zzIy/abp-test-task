namespace Domain.Entities;

public class ConferenceRoom
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public int Capacity { get; set; }

    public decimal HourlyRate { get; set; }

    public ICollection<Service> Services { get; set; }
        = new List<Service>();

    public ICollection<Booking> Bookings { get; set; }
        = new List<Booking>();
}