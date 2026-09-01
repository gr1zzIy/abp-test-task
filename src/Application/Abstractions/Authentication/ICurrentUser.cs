namespace Application.Abstractions.Authentication;

/// <summary>
/// Надає дані поточного автентифікованого користувача
/// без залежності Application від HTTP або ASP.NET Core.
/// </summary>
public interface ICurrentUser
{
    bool IsAuthenticated { get; }

    Guid? UserId { get; }

    string? Email { get; }

    IReadOnlyCollection<string> Roles { get; }
}