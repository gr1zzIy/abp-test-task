namespace Application.Abstractions.Authentication;

/// <summary>
/// Надає операції для створення користувачів
/// та перевірки їх облікових даних.
/// </summary>
public interface IIdentityService
{
    Task<bool> EmailExistsAsync(
        string email,
        CancellationToken cancellationToken = default);

    Task<CreateUserResult> CreateUserAsync(
        string email,
        string password,
        string role,
        CancellationToken cancellationToken = default);

    Task<AuthenticatedUser?> ValidateCredentialsAsync(
        string email,
        string password,
        CancellationToken cancellationToken = default);
}

public sealed record CreateUserResult(
    bool Succeeded,
    Guid? UserId,
    IReadOnlyCollection<string> Errors);

public sealed record AuthenticatedUser(
    Guid Id,
    string Email,
    IReadOnlyCollection<string> Roles);