namespace Infrastructure.Identity;

internal sealed class AdminOptions
{
    public const string SectionName = "Admin";

    public bool SeedOnStartup { get; init; }

    public string Email { get; init; } = string.Empty;

    public string Password { get; init; } = string.Empty;
}