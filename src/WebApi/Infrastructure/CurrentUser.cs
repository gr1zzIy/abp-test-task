using System.Security.Claims;
using Application.Abstractions.Authentication;

namespace WebApi.Infrastructure;

/// <summary>
/// Отримує дані поточного користувача з claims,
/// сформованих після перевірки JWT access token.
/// </summary>
public sealed class CurrentUser : ICurrentUser
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUser(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private ClaimsPrincipal User =>
        _httpContextAccessor.HttpContext?.User
        ?? new ClaimsPrincipal();

    public bool IsAuthenticated =>
        User.Identity?.IsAuthenticated == true;

    public Guid? UserId
    {
        get
        {
            var value = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            return Guid.TryParse(value, out var userId)
                ? userId
                : null;
        }
    }

    public string? Email =>
        User.FindFirstValue(ClaimTypes.Email)
        ?? User.FindFirstValue("email");

    public IReadOnlyCollection<string> Roles =>
        User.FindAll(ClaimTypes.Role)
            .Select(claim => claim.Value)
            .ToArray();
}