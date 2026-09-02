using System.ComponentModel.DataAnnotations;

namespace Infrastructure.Options;

public sealed class BookingOptions
{
    public const string SectionName = "Booking";

    [Required(ErrorMessage = "Booking TimeZone must be configured.")]
    public string TimeZone { get; set; } = string.Empty;
}