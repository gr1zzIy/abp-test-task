using Domain.Entities;
using Domain.Enums;
using Infrastructure.Identity;
using Infrastructure.Persistence;
using Integration.Tests.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

namespace Integration.Tests.Persistence;

[Collection(IntegrationTestCollection.Name)]
public sealed class BookingOverlapConstraintTests
{
    private static readonly Guid RoomId =
        Guid.Parse("22222222-2222-2222-2222-222222222222");

    private readonly IntegrationTestFixture _fixture;

    public BookingOverlapConstraintTests(
        IntegrationTestFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task SaveChanges_OverlappingActiveBookings_ThrowsExclusionViolation()
    {
        var userId = await GetAdminUserIdAsync();

        /*
         * Використовуємо окремий часовий проміжок, щоб тест
         * не залежав від бронювань інших integration tests.
         */
        var startTime = new DateTimeOffset(
            2032, 3, 10,
            8, 0, 0,
            TimeSpan.Zero);

        var endTime = new DateTimeOffset(
            2032, 3, 10,
            10, 0, 0,
            TimeSpan.Zero);

        await CreateBookingDirectlyAsync(
            new Booking
            {
                Id = Guid.NewGuid(),
                ConferenceRoomId = RoomId,
                UserId = userId,
                StartTime = startTime,
                EndTime = endTime,
                TotalPrice = 2000m,
                Status = BookingStatus.Active
            });

        var overlappingBooking = new Booking
        {
            Id = Guid.NewGuid(),
            ConferenceRoomId = RoomId,
            UserId = userId,

            // Частково перетинається з існуючим бронюванням.
            StartTime = startTime.AddHours(1),
            EndTime = endTime.AddHours(1),

            TotalPrice = 2000m,
            Status = BookingStatus.Active
        };

        using var scope =
            _fixture.Factory.Services.CreateScope();

        var dbContext = scope.ServiceProvider
            .GetRequiredService<AppDbContext>();

        await dbContext.Bookings.AddAsync(
            overlappingBooking);

        var exception =
            await Assert.ThrowsAsync<DbUpdateException>(
                () => dbContext.SaveChangesAsync());

        var postgresException =
            Assert.IsType<PostgresException>(
                exception.InnerException);

        Assert.Equal(
            PostgresErrorCodes.ExclusionViolation,
            postgresException.SqlState);

        Assert.Equal(
            "ex_bookings_no_overlap",
            postgresException.ConstraintName);
    }

    private async Task CreateBookingDirectlyAsync(
        Booking booking)
    {
        using var scope =
            _fixture.Factory.Services.CreateScope();

        var dbContext = scope.ServiceProvider
            .GetRequiredService<AppDbContext>();

        await dbContext.Bookings.AddAsync(booking);
        await dbContext.SaveChangesAsync();
    }

    private async Task<Guid> GetAdminUserIdAsync()
    {
        using var scope =
            _fixture.Factory.Services.CreateScope();

        var userManager = scope.ServiceProvider
            .GetRequiredService<UserManager<ApplicationUser>>();

        var admin = await userManager.FindByEmailAsync(
            "admin@integration.test");

        return admin?.Id
               ?? throw new InvalidOperationException(
                   "Integration test administrator was not created.");
    }
}