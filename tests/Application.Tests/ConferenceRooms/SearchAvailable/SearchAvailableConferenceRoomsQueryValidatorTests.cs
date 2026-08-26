using Application.ConferenceRooms.SearchAvailable;

namespace Application.Tests.ConferenceRooms.SearchAvailable;

public sealed class SearchAvailableConferenceRoomsQueryValidatorTests
{
    private readonly SearchAvailableConferenceRoomsQueryValidator _validator = new();

    [Fact]
    public void Validate_ValidQuery_HasNoErrors()
    {
        var query = CreateQuery();

        var result = _validator.Validate(query);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_ZeroCapacity_HasValidationError()
    {
        var query = CreateQuery(capacity: 0);

        var result = _validator.Validate(query);

        Assert.Contains(
            result.Errors,
            error => error.PropertyName ==
                     nameof(SearchAvailableConferenceRoomsQuery.Capacity));
    }

    [Fact]
    public void Validate_NegativeCapacity_HasValidationError()
    {
        var query = CreateQuery(capacity: -10);

        var result = _validator.Validate(query);

        Assert.False(result.IsValid);
    }

    [Fact]
    public void Validate_EndTimeBeforeStartTime_HasValidationError()
    {
        var query = new SearchAvailableConferenceRoomsQuery(
            CreateDateTime(14),
            CreateDateTime(10),
            50);

        var result = _validator.Validate(query);

        Assert.False(result.IsValid);
    }

    [Fact]
    public void Validate_EqualStartAndEndTime_HasValidationError()
    {
        var time = CreateDateTime(10);

        var query = new SearchAvailableConferenceRoomsQuery(
            time,
            time,
            50);

        var result = _validator.Validate(query);

        Assert.False(result.IsValid);
    }

    private static SearchAvailableConferenceRoomsQuery CreateQuery(
        int capacity = 50)
    {
        return new SearchAvailableConferenceRoomsQuery(
            CreateDateTime(10),
            CreateDateTime(14),
            capacity);
    }

    private static DateTimeOffset CreateDateTime(int hour)
    {
        return new DateTimeOffset(
            2026,
            9,
            1,
            hour,
            0,
            0,
            TimeSpan.FromHours(3));
    }
}