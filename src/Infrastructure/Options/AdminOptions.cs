using System.ComponentModel.DataAnnotations;

namespace Infrastructure.Options;

public sealed class AdminOptions
{
    public const string SectionName = "Admin";

    public bool SeedOnStartup { get; set; }

    [EmailAddress(ErrorMessage = "Invalid Admin email address.")]
    public string Email { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;
}