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
            error => error.PropertyName ==
                     nameof(CreateBookingCommand.ConferenceRoomId));
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