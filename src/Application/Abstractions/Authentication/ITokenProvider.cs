namespace Application.Abstractions.Authentication;

/// <summary>
/// Створює access token для автентифікованого користувача.
/// </summary>
public interface ITokenProvider
{
    string CreateAccessToken(
        Guid userId,
        string email,
        IReadOnlyCollection<string> roles);
}