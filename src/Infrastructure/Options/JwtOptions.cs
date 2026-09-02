using System.ComponentModel.DataAnnotations;

namespace Infrastructure.Options;

public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    [Required(ErrorMessage = "JWT Issuer is required.")]
    public string Issuer { get; set; } = string.Empty;

    [Required(ErrorMessage = "JWT Audience is required.")]
    public string Audience { get; set; } = string.Empty;

    [Required(ErrorMessage = "JWT Key is required.")]
    [MinLength(32, ErrorMessage = "JWT key must contain at least 32 characters.")]
    public string Key { get; set; } = string.Empty;

    [Range(1, 10080, ErrorMessage = "Expiration must be between 1 minute and 7 days.")]
    public int ExpirationMinutes { get; set; } = 60;
}