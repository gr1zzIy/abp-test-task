using Application.Abstractions.Time;

namespace Infrastructure.Time;

internal sealed class BusinessTimeZone : IBusinessTimeZone
{
    private readonly TimeZoneInfo _timeZone;

    public BusinessTimeZone(string timeZoneId)
    {
        if (string.IsNullOrWhiteSpace(timeZoneId))
        {
            throw new ArgumentException(
                "Business time zone id must be configured.",
                nameof(timeZoneId));
        }

        _timeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
    }

    public DateTimeOffset ConvertFromUtc(DateTimeOffset utcTime)
    {
        return TimeZoneInfo.ConvertTime(
            utcTime.ToUniversalTime(),
            _timeZone);
    }
}