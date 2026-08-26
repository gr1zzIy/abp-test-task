using Application.Bookings.Create;

namespace Application.Tests.Bookings.Create;

public sealed class CreateBookingCommandValidatorTests
{
    private readonly CreateBookingCommandValidator _validator = new();

    [Fact]
    public void Validate_ValidCommand_HasNoErrors()
    {
        var command = CreateCommand(
            startHour: 10,
            endHour: 12);

        var result = _validator.Validate(command);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_EmptyConferenceRoomId_HasValidationError()
    {
        var command = CreateCommand(
            conferenceRoomId: Guid.Empty);

        var result = _validator.Validate(command);

        Assert.Contains(
            result.Errors,
            error => error.PropertyName == nameof(CreateBookingCommand.ConferenceRoomId));
    }

    [Fact]
    public void Validate_EndTimeBeforeStartTime_HasValidationError()
    {
        var command = CreateCommand(
            startHour: 12,
            endHour: 10);

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
    }

    [Fact]
    public void Validate_BookingBeforeWorkingHours_HasValidationError()
    {
        var command = CreateCommand(
            startHour: 5,
            endHour: 8);

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
    }

    [Fact]
    public void Validate_BookingAfterWorkingHours_HasValidationError()
    {
        var command = new CreateBookingCommand(
            Guid.NewGuid(),
            CreateDateTime(22),
            new DateTimeOffset(
                2026,
                9,
                1,
                23,
                30,
                0,
                TimeSpan.FromHours(3)),
            []);

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
    }

    [Fact]
    public void Validate_BookingAcrossDifferentDays_HasValidationError()
    {
        var startTime = CreateDateTime(22);
        var endTime = CreateDateTime(8).AddDays(1);

        var command = new CreateBookingCommand(
            Guid.NewGuid(),
            startTime,
            endTime,
            []);

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
    }

    [Theory]
    [InlineData(6, 9)]
    [InlineData(9, 12)]
    [InlineData(12, 14)]
    [InlineData(18, 23)]
    public void Validate_BookingOnAllowedBoundaries_HasNoErrors(
        int startHour,
        int endHour)
    {
        var command = CreateCommand(
            startHour: startHour,
            endHour: endHour);

        var result = _validator.Validate(command);

        Assert.True(result.IsValid);
    }

    private static CreateBookingCommand CreateCommand(
        Guid? conferenceRoomId = null,
        int startHour = 10,
        int endHour = 12)
    {
        return new CreateBookingCommand(
            conferenceRoomId ?? Guid.NewGuid(),
            CreateDateTime(startHour),
            CreateDateTime(endHour),
            []);
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