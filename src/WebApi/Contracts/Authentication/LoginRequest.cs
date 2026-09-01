namespace WebApi.Contracts.Authentication;

/// <summary>
/// Облікові дані для входу в систему.
/// </summary>
public sealed record LoginRequest(
    string Email,
    string Password);