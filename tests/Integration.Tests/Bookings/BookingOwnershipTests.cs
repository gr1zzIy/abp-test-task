using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Infrastructure.Persistence;
using Integration.Tests.Helpers;
using Integration.Tests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Integration.Tests.Bookings;

[Collection(IntegrationTestCollection.Name)]
public sealed class BookingOwnershipTests
{
    private static readonly Guid RoomAId =
        Guid.Parse("11111111-1111-1111-1111-111111111111");

    private static readonly Guid RoomBId =
        Guid.Parse("22222222-2222-2222-2222-222222222222");

    private static readonly Guid RoomCId =
        Guid.Parse("33333333-3333-3333-3333-333333333333");

    private readonly IntegrationTestFixture _fixture;
    private readonly HttpClient _client;

    public BookingOwnershipTests(
        IntegrationTestFixture fixture)
    {
        _fixture = fixture;
        _client = fixture.Client;
    }

    [Fact]
    public async Task GetBooking_Owner_ReturnsBooking()
    {
        var email =
            $"owner-{Guid.NewGuid():N}@integration.test";

        await AuthenticationTestHelper.RegisterAsync(
            _client,
            email);

        var accessToken =
            await AuthenticationTestHelper.LoginAsync(
                _client,
                email);

        var bookingId = await CreateBookingAsync(
            accessToken,
            RoomAId,
            new DateTimeOffset(
                2030, 1, 15,
                8, 0, 0,
                TimeSpan.Zero),
            new DateTimeOffset(
                2030, 1, 15,
                10, 0, 0,
                TimeSpan.Zero));

        using var request = new HttpRequestMessage(
                HttpMethod.Get,
                $"/api/bookings/{bookingId}")
            .WithBearerToken(accessToken);

        var response = await _client.SendAsync(request);

        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);
    }

    [Fact]
    public async Task GetBooking_DifferentClient_ReturnsNotFound()
    {
        var ownerEmail =
            $"owner-{Guid.NewGuid():N}@integration.test";

        var otherClientEmail =
            $"other-{Guid.NewGuid():N}@integration.test";

        await AuthenticationTestHelper.RegisterAsync(
            _client,
            ownerEmail);

        await AuthenticationTestHelper.RegisterAsync(
            _client,
            otherClientEmail);

        var ownerToken =
            await AuthenticationTestHelper.LoginAsync(
                _client,
                ownerEmail);

        var otherClientToken =
            await AuthenticationTestHelper.LoginAsync(
                _client,
                otherClientEmail);

        var bookingId = await CreateBookingAsync(
            ownerToken,
            RoomBId,
            new DateTimeOffset(
                2030, 1, 16,
                8, 0, 0,
                TimeSpan.Zero),
            new DateTimeOffset(
                2030, 1, 16,
                10, 0, 0,
                TimeSpan.Zero));

        using var request = new HttpRequestMessage(
                HttpMethod.Get,
                $"/api/bookings/{bookingId}")
            .WithBearerToken(otherClientToken);

        var response = await _client.SendAsync(request);

        // Не повертаємо 403, щоб не розкривати
        // існування бронювання іншого користувача.
        Assert.Equal(
            HttpStatusCode.NotFound,
            response.StatusCode);
    }

    [Fact]
    public async Task CreateBooking_AssignsAuthenticatedUserAsOwner()
    {
        var email =
            $"owner-{Guid.NewGuid():N}@integration.test";

        var registeredUser =
            await AuthenticationTestHelper.RegisterAsync(
                _client,
                email);

        var accessToken =
            await AuthenticationTestHelper.LoginAsync(
                _client,
                email);

        var bookingId = await CreateBookingAsync(
            accessToken,
            RoomCId,
            new DateTimeOffset(
                2030, 1, 17,
                8, 0, 0,
                TimeSpan.Zero),
            new DateTimeOffset(
                2030, 1, 17,
                10, 0, 0,
                TimeSpan.Zero));

        using var scope =
            _fixture.Factory.Services.CreateScope();

        var dbContext = scope.ServiceProvider
            .GetRequiredService<AppDbContext>();

        var booking = await dbContext.Bookings
            .AsNoTracking()
            .SingleAsync(
                booking => booking.Id == bookingId);

        // Власник бронювання визначається з JWT-контексту,
        // а не з даних, які надсилає клієнт.
        Assert.Equal(
            registeredUser.Id,
            booking.UserId);
    }

    [Fact]
    public async Task GetBookings_Client_ReturnsOnlyOwnBookings()
    {
        var firstClientEmail =
            $"first-{Guid.NewGuid():N}@integration.test";

        var secondClientEmail =
            $"second-{Guid.NewGuid():N}@integration.test";

        await AuthenticationTestHelper.RegisterAsync(
            _client,
            firstClientEmail);

        await AuthenticationTestHelper.RegisterAsync(
            _client,
            secondClientEmail);

        var firstClientToken =
            await AuthenticationTestHelper.LoginAsync(
                _client,
                firstClientEmail);

        var secondClientToken =
            await AuthenticationTestHelper.LoginAsync(
                _client,
                secondClientEmail);

        var firstClientBookingId = await CreateBookingAsync(
            firstClientToken,
            RoomAId,
            new DateTimeOffset(
                2030, 1, 18,
                8, 0, 0,
                TimeSpan.Zero),
            new DateTimeOffset(
                2030, 1, 18,
                10, 0, 0,
                TimeSpan.Zero));

        var secondClientBookingId = await CreateBookingAsync(
            secondClientToken,
            RoomBId,
            new DateTimeOffset(
                2030, 1, 18,
                8, 0, 0,
                TimeSpan.Zero),
            new DateTimeOffset(
                2030, 1, 18,
                10, 0, 0,
                TimeSpan.Zero));

        using var request = new HttpRequestMessage(
                HttpMethod.Get,
                "/api/bookings")
            .WithBearerToken(firstClientToken);

        var response = await _client.SendAsync(request);

        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);

        using var document = JsonDocument.Parse(
            await response.Content.ReadAsStringAsync());

        var bookingIds = document.RootElement
            .EnumerateArray()
            .Select(item => item
                .GetProperty("id")
                .GetGuid())
            .ToArray();

        Assert.Contains(
            firstClientBookingId,
            bookingIds);

        Assert.DoesNotContain(
            secondClientBookingId,
            bookingIds);
    }

    private async Task<Guid> CreateBookingAsync(
        string accessToken,
        Guid conferenceRoomId,
        DateTimeOffset startTime,
        DateTimeOffset endTime)
    {
        using var request = new HttpRequestMessage(
                HttpMethod.Post,
                "/api/bookings")
            .WithBearerToken(accessToken);

        request.Content = JsonContent.Create(
            new
            {
                ConferenceRoomId = conferenceRoomId,
                StartTime = startTime,
                EndTime = endTime,
                ServiceIds = Array.Empty<int>()
            });

        var response = await _client.SendAsync(request);

        Assert.Equal(
            HttpStatusCode.Created,
            response.StatusCode);

        using var document = JsonDocument.Parse(
            await response.Content.ReadAsStringAsync());

        return document.RootElement
            .GetProperty("id")
            .GetGuid();
    }
}