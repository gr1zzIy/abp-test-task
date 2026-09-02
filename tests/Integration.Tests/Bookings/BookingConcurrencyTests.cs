using System.Net;
using System.Net.Http.Json;
using Integration.Tests.Helpers;
using Integration.Tests.Infrastructure;

namespace Integration.Tests.Bookings;

[Collection(IntegrationTestCollection.Name)]
public sealed class BookingConcurrencyTests
{
    private static readonly Guid RoomId =
        Guid.Parse("33333333-3333-3333-3333-333333333333");

    private readonly HttpClient _client;

    public BookingConcurrencyTests(
        IntegrationTestFixture fixture)
    {
        _client = fixture.Client;
    }

    [Fact]
    public async Task CreateBooking_ConcurrentRequestsForSameSlot_AllowsOnlyOneBooking()
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

        var firstToken =
            await AuthenticationTestHelper.LoginAsync(
                _client,
                firstClientEmail);

        var secondToken =
            await AuthenticationTestHelper.LoginAsync(
                _client,
                secondClientEmail);

        /*
         * ” лютому Europe/Kyiv маЇ UTC+2:
         * 08:00ZЦ10:00Z в≥дпов≥даЇ 10:00Ц12:00 за б≥знес-часом.
         */
        var startTime = new DateTimeOffset(
            2031, 2, 3,
            8, 0, 0,
            TimeSpan.Zero);

        var endTime = new DateTimeOffset(
            2031, 2, 3,
            10, 0, 0,
            TimeSpan.Zero);

        var firstRequest = CreateBookingRequest(
            firstToken,
            startTime,
            endTime);

        var secondRequest = CreateBookingRequest(
            secondToken,
            startTime,
            endTime);

        // ќбидва HTTP-запити запускаютьс€ до оч≥куванн€ результат≥в,
        // щоб максимально наблизити тест до конкурентного сценар≥ю.
        var firstTask = _client.SendAsync(firstRequest);
        var secondTask = _client.SendAsync(secondRequest);

        var responses = await Task.WhenAll(
            firstTask,
            secondTask);

        try
        {
            var createdCount = responses.Count(
                response =>
                    response.StatusCode ==
                    HttpStatusCode.Created);

            var conflictCount = responses.Count(
                response =>
                    response.StatusCode ==
                    HttpStatusCode.Conflict);

            Assert.Equal(1, createdCount);
            Assert.Equal(1, conflictCount);
        }
        finally
        {
            foreach (var response in responses)
            {
                response.Dispose();
            }

            firstRequest.Dispose();
            secondRequest.Dispose();
        }
    }

    private static HttpRequestMessage CreateBookingRequest(
        string accessToken,
        DateTimeOffset startTime,
        DateTimeOffset endTime)
    {
        var request = new HttpRequestMessage(
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

        return request;
    }
}