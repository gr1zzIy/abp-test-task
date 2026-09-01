using Application.Abstractions.Authentication;
using Application.Authentication.Me;
using Application.Common.Exceptions;
using Moq;

namespace Application.Tests.Authentication.Me;

public sealed class GetCurrentUserHandlerTests
{
    private readonly Mock<ICurrentUser> _currentUser = new();

    private readonly GetCurrentUserHandler _handler;

    public GetCurrentUserHandlerTests()
    {
        _handler = new GetCurrentUserHandler(
            _currentUser.Object);
    }

    [Fact]
    public void Handle_AuthenticatedUser_ReturnsCurrentUser()
    {
        var userId = Guid.NewGuid();
        var roles = new[] { "Client" };

        _currentUser
            .SetupGet(x => x.IsAuthenticated)
            .Returns(true);

        _currentUser
            .SetupGet(x => x.UserId)
            .Returns(userId);

        _currentUser
            .SetupGet(x => x.Email)
            .Returns("client@example.com");

        _currentUser
            .SetupGet(x => x.Roles)
            .Returns(roles);

        var result = _handler.Handle();

        Assert.Equal(userId, result.Id);
        Assert.Equal("client@example.com", result.Email);
        Assert.Equal(roles, result.Roles);
    }

    [Fact]
    public void Handle_UnauthenticatedUser_ThrowsUnauthorizedException()
    {
        _currentUser
            .SetupGet(x => x.IsAuthenticated)
            .Returns(false);

        Assert.Throws<UnauthorizedException>(() =>
            _handler.Handle());
    }

    [Fact]
    public void Handle_AuthenticatedUserWithoutId_ThrowsUnauthorizedException()
    {
        _currentUser
            .SetupGet(x => x.IsAuthenticated)
            .Returns(true);

        _currentUser
            .SetupGet(x => x.UserId)
            .Returns((Guid?)null);

        _currentUser
            .SetupGet(x => x.Email)
            .Returns("client@example.com");

        Assert.Throws<UnauthorizedException>(() =>
            _handler.Handle());
    }

    [Fact]
    public void Handle_AuthenticatedUserWithoutEmail_ThrowsUnauthorizedException()
    {
        _currentUser
            .SetupGet(x => x.IsAuthenticated)
            .Returns(true);

        _currentUser
            .SetupGet(x => x.UserId)
            .Returns(Guid.NewGuid());

        _currentUser
            .SetupGet(x => x.Email)
            .Returns((string?)null);

        Assert.Throws<UnauthorizedException>(() =>
            _handler.Handle());
    }
}