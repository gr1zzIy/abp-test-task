using Application.Abstractions.Authentication;
using Application.Common.Exceptions;

namespace Application.Authentication.Me;

/// <summary>
/// Повертає дані користувача, автентифікованого поточним JWT token.
/// </summary>
public sealed class GetCurrentUserHandler
{
    private readonly ICurrentUser _currentUser;

    public GetCurrentUserHandler(ICurrentUser currentUser)
    {
        _currentUser = currentUser;
    }

    public CurrentUserResult Handle()
    {
        if (!_currentUser.IsAuthenticated ||
            _currentUser.UserId is null ||
            string.IsNullOrWhiteSpace(_currentUser.Email))
        {
            throw new UnauthorizedException(
                "User is not authenticated.");
        }

        return new CurrentUserResult(
            _currentUser.UserId.Value,
            _currentUser.Email,
            _currentUser.Roles);
    }
}