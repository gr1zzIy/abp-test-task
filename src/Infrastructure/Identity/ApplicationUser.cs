using Microsoft.AspNetCore.Identity;

namespace Infrastructure.Identity;

/// <summary>
/// Представляє користувача системи автентифікації.
/// </summary>
public sealed class ApplicationUser : IdentityUser<Guid>
{
}