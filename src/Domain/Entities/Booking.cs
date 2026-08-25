namespace Domain.Entities;

public class Booking
{
    public Guid Id { get; set; }

    public DateTimeOffset StartTime { get; set; }
    public DateTimeOffset EndTime { get; set; }

    public decimal TotalPrice { get; set; }

    public Guid ConferenceRoomId { get; set; }
    public ConferenceRoom ConferenceRoom { get; set; } = null!;

    public ICollection<Service> SelectedServices { get; set; } = new List<Service>();
}