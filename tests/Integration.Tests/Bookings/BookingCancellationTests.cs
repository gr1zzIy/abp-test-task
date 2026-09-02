using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Integration.Tests.Helpers;
using Integration.Tests.Infrastructure;

namespace Integration.Tests.Bookings;

[Collection(IntegrationTestCollection.Name)]
public sealed class BookingCancellationTests
{
    private static readonly Guid RoomId =
        Guid.Parse("11111111-1111-1111-1111-111111111111");

    private readonly HttpClient _client;

    public BookingCancellationTests(
        IntegrationTestFixture fixture)
    {
        _client = fixture.Client;
    }

    [Fact]
    public async Task CancelBooking_ReleasesSlot_AndAllowsBookingAgain()
    {
        var email =
            $"client-{Guid.NewGuid():N}@integration.test";

        await AuthenticationTestHelper.RegisterAsync(
            _client,
            email);

        var accessToken =
            await AuthenticationTestHelper.LoginAsync(
                _client,
                email);

        /*
         * У лютому Europe/Kyiv має UTC+2:
         * 08:00Z–10:00Z відповідає 10:00–12:00 за бізнес-часом.
         */
        var startTime = new DateTimeOffset(
            2031, 2, 4,
            8, 0, 0,
            TimeSpan.Zero);

        var endTime = new DateTimeOffset(
            2031, 2, 4,
            10, 0, 0,
            TimeSpan.Zero);

        var bookingId = await CreateBookingAsync(
            accessToken,
            startTime,
            endTime);

        var availableBeforeCancellation =
            await GetAvailableRoomIdsAsync(
                startTime,
                endTime);

        Assert.DoesNotContain(
            RoomId,
            availableBeforeCancellation);

        using var cancelRequest = new HttpRequestMessage(
                HttpMethod.Post,
                $"/api/bookings/{bookingId}/cancel")
            .WithBearerToken(accessToken);

        var cancelResponse =
            await _client.SendAsync(cancelRequest);

        Assert.Equal(
            HttpStatusCode.NoContent,
            cancelResponse.StatusCode);

        var availableAfterCancellation =
            await GetAvailableRoomIdsAsync(
                startTime,
                endTime);

        Assert.Contains(
            RoomId,
            availableAfterCancellation);

        // Повторне бронювання того самого слота доводить,
        // що скасований запис більше не блокується
        // PostgreSQL exclusion constraint.
        var secondBookingId = await CreateBookingAsync(
            accessToken,
            startTime,
            endTime);

        Assert.NotEqual(
            bookingId,
            secondBookingId);
    }

    private async Task<Guid> CreateBookingAsync(
        string accessToken,
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
                ConferenceRoomId = RoomId,
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

    private async Task<Guid[]> GetAvailableRoomIdsAsync(
        DateTimeOffset startTime,
        DateTimeOffset endTime)
    {
        var url =
            $"/api/conference-rooms/available" +
            $"?startTime={Uri.EscapeDataString(startTime.ToString("O"))}" +
            $"&endTime={Uri.EscapeDataString(endTime.ToString("O"))}" +
            $"&capacity=1";

        var response = await _client.GetAsync(url);

        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);

        using var document = JsonDocument.Parse(
            await response.Content.ReadAsStringAsync());

        return document.RootElement
            .EnumerateArray()
            .Select(room => room
                .GetProperty("id")
                .GetGuid())
            .ToArray();
    }
}