using Application.Abstractions.Authentication;
using Application.Common.Exceptions;
using FluentValidation;

namespace Application.Authentication.Login;

/// <summary>
/// Перевіряє облікові дані користувача та створює JWT access token.
/// </summary>
public sealed class LoginHandler
{
    private readonly IIdentityService _identityService;
    private readonly ITokenProvider _tokenProvider;
    private readonly IValidator<LoginCommand> _validator;

    public LoginHandler(
        IIdentityService identityService,
        ITokenProvider tokenProvider,
        IValidator<LoginCommand> validator)
    {
        _identityService = identityService;
        _tokenProvider = tokenProvider;
        _validator = validator;
    }

    public async Task<LoginResult> HandleAsync(
        LoginCommand command,
        CancellationToken cancellationToken = default)
    {
        await _validator.ValidateAndThrowAsync(
            command,
            cancellationToken);

        var user = await _identityService.ValidateCredentialsAsync(
            command.Email.Trim(),
            command.Password,
            cancellationToken);

        // Для неправильного email і неправильного пароля повертаємо однакову
        // відповідь, щоб не розкривати наявність конкретного користувача.
        if (user is null)
        {
            throw new UnauthorizedException(
                "Invalid email or password.");
        }

        var accessToken = _tokenProvider.CreateAccessToken(
            user.Id,
            user.Email,
            user.Roles);

        return new LoginResult(accessToken);
    }
}