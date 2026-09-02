using Application.Abstractions.Time;
using Infrastructure.Options;
using Microsoft.Extensions.Options;
using TimeZoneConverter;

namespace Infrastructure.Time;

internal sealed class BusinessTimeZone : IBusinessTimeZone
{
    private readonly TimeZoneInfo _timeZone;

    public BusinessTimeZone(IOptions<BookingOptions> options)
    {
        var timeZoneId = options.Value.TimeZone;
        _timeZone = TZConvert.GetTimeZoneInfo(timeZoneId);
    }

    public DateTimeOffset ConvertFromUtc(DateTimeOffset utcTime)
    {
        return TimeZoneInfo.ConvertTime(
            utcTime.ToUniversalTime(),
            _timeZone);
    }
}