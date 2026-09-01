using Application.Abstractions.Authentication;
using Application.Authentication.Login;
using Application.Common.Exceptions;
using FluentValidation;
using FluentValidation.Results;
using Moq;

namespace Application.Tests.Authentication.Login;

public sealed class LoginHandlerTests
{
    private readonly Mock<IIdentityService> _identityService = new();
    private readonly Mock<ITokenProvider> _tokenProvider = new();
    private readonly Mock<IValidator<LoginCommand>> _validator = new();

    private readonly LoginHandler _handler;

    public LoginHandlerTests()
    {
        _handler = new LoginHandler(
            _identityService.Object,
            _tokenProvider.Object,
            _validator.Object);

        _validator
            .Setup(x => x.ValidateAsync(
                It.IsAny<LoginCommand>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());
    }

    [Fact]
    public async Task HandleAsync_InvalidCredentials_ThrowsUnauthorizedException()
    {
        var command = new LoginCommand(
            "client@example.com",
            "WrongPassword123");

        _identityService
            .Setup(x => x.ValidateCredentialsAsync(
                command.Email,
                command.Password,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((AuthenticatedUser?)null);

        await Assert.ThrowsAsync<UnauthorizedException>(() =>
            _handler.HandleAsync(command));

        _tokenProvider.Verify(
            x => x.CreateAccessToken(
                It.IsAny<Guid>(),
                It.IsAny<string>(),
                It.IsAny<IReadOnlyCollection<string>>()),
            Times.Never);
    }

    [Fact]
    public async Task HandleAsync_ValidCredentials_ReturnsAccessToken()
    {
        var command = new LoginCommand(
            "client@example.com",
            "Password123");

        var user = new AuthenticatedUser(
            Guid.NewGuid(),
            command.Email,
            ["Client"]);

        const string accessToken = "generated-jwt-token";

        _identityService
            .Setup(x => x.ValidateCredentialsAsync(
                command.Email,
                command.Password,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _tokenProvider
            .Setup(x => x.CreateAccessToken(
                user.Id,
                user.Email,
                user.Roles))
            .Returns(accessToken);

        var result = await _handler.HandleAsync(command);

        Assert.Equal(accessToken, result.AccessToken);

        // Handler повинен передати в token provider саме дані
        // автентифікованого користувача разом із його ролями.
        _tokenProvider.Verify(
            x => x.CreateAccessToken(
                user.Id,
                user.Email,
                user.Roles),
            Times.Once);
    }
}