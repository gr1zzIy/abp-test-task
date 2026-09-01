using Application.Common.Security;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Identity.Configurations;

public sealed class RoleConfiguration
    : IEntityTypeConfiguration<IdentityRole<Guid>>
{
    public void Configure(
        EntityTypeBuilder<IdentityRole<Guid>> builder)
    {
        // Фіксовані ідентифікатори забезпечують стабільний seed ролей
        // між наступними EF Core міграціями.
        builder.HasData(
            new IdentityRole<Guid>
            {
                Id = Guid.Parse(
                    "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                Name = Roles.Admin,
                NormalizedName = Roles.Admin.ToUpperInvariant(),
                ConcurrencyStamp =
                    "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"
            },
            new IdentityRole<Guid>
            {
                Id = Guid.Parse(
                    "bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),
                Name = Roles.Client,
                NormalizedName = Roles.Client.ToUpperInvariant(),
                ConcurrencyStamp =
                    "bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"
            });
    }
}