using Application.Abstractions.Authentication;
using Application.Authentication.Register;
using Application.Common.Exceptions;
using Application.Common.Security;
using FluentValidation;
using FluentValidation.Results;
using Moq;

namespace Application.Tests.Authentication.Register;

public sealed class RegisterHandlerTests
{
    private readonly Mock<IIdentityService> _identityService = new();
    private readonly Mock<IValidator<RegisterCommand>> _validator = new();

    private readonly RegisterHandler _handler;

    public RegisterHandlerTests()
    {
        _handler = new RegisterHandler(
            _identityService.Object,
            _validator.Object);

        _validator
            .Setup(x => x.ValidateAsync(
                It.IsAny<RegisterCommand>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());
    }

    [Fact]
    public async Task HandleAsync_EmailAlreadyExists_ThrowsConflictException()
    {
        var command = new RegisterCommand(
            "client@example.com",
            "Password123");

        _identityService
            .Setup(x => x.EmailExistsAsync(
                command.Email,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        await Assert.ThrowsAsync<ConflictException>(() =>
            _handler.HandleAsync(command));
    }

    [Fact]
    public async Task HandleAsync_ValidCommand_CreatesClientUser()
    {
        var command = new RegisterCommand(
            "client@example.com",
            "Password123");

        var userId = Guid.NewGuid();

        _identityService
            .Setup(x => x.EmailExistsAsync(
                command.Email,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _identityService
            .Setup(x => x.CreateUserAsync(
                command.Email,
                command.Password,
                Roles.Client,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                new CreateUserResult(
                    true,
                    userId,
                    []));

        var result = await _handler.HandleAsync(command);

        Assert.Equal(userId, result.Id);
        Assert.Equal(command.Email, result.Email);

        // Публічна реєстрація повинна створювати
        // виключно користувача з роллю Client.
        _identityService.Verify(
            x => x.CreateUserAsync(
                command.Email,
                command.Password,
                Roles.Client,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}