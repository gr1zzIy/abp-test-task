namespace Application.Authentication.Me;

/// <summary>
/// Дані поточного автентифікованого користувача.
/// </summary>
public sealed record CurrentUserResult(
    Guid Id,
    string Email,
    IReadOnlyCollection<string> Roles);