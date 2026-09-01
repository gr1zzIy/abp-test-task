using Application.Abstractions.Authentication;
using Application.Common.Exceptions;
using Application.Common.Security;
using FluentValidation;

namespace Application.Authentication.Register;

/// <summary>
/// Реалізує реєстрацію нового клієнта системи.
/// Публічна реєстрація завжди створює користувача з роллю Client.
/// </summary>
public sealed class RegisterHandler
{
    private readonly IIdentityService _identityService;
    private readonly IValidator<RegisterCommand> _validator;

    public RegisterHandler(
        IIdentityService identityService,
        IValidator<RegisterCommand> validator)
    {
        _identityService = identityService;
        _validator = validator;
    }

    public async Task<RegisterResult> HandleAsync(
        RegisterCommand command,
        CancellationToken cancellationToken = default)
    {
        await _validator.ValidateAndThrowAsync(
            command,
            cancellationToken);

        var email = command.Email.Trim();

        var emailExists = await _identityService.EmailExistsAsync(
            email,
            cancellationToken);

        if (emailExists)
        {
            throw new ConflictException(
                "User with this email already exists.");
        }

        // Роль не приймається від клієнта, щоб через публічний endpoint
        // неможливо було самостійно зареєструвати обліковий запис адміністратора.
        var result = await _identityService.CreateUserAsync(
            email,
            command.Password,
            Roles.Client,
            cancellationToken);

        if (!result.Succeeded || result.UserId is null)
        {
            throw new BadRequestException(
                string.Join(" ", result.Errors));
        }

        return new RegisterResult(
            result.UserId.Value,
            email);
    }
}