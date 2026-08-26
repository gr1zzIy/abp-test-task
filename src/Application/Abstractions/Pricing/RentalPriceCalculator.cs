using Application.Abstractions.Pricing;

namespace Application.Pricing;

/// <summary>
/// Розраховує вартість оренди з урахуванням часових тарифів.
/// Якщо бронювання охоплює декілька тарифних періодів,
/// кожна частина розраховується окремо.
/// </summary>
public sealed class RentalPriceCalculator : IRentalPriceCalculator
{
    public decimal Calculate(
        decimal hourlyRate,
        DateTimeOffset startTime,
        DateTimeOffset endTime)
    {
        if (endTime <= startTime)
        {
            throw new ArgumentException(
                "Час завершення повинен бути пізніше часу початку.");
        }

        decimal totalPrice = 0;
        var current = startTime;

        while (current < endTime)
        {
            var nextBoundary = GetNextBoundary(current);

            if (nextBoundary > endTime)
            {
                nextBoundary = endTime;
            }

            var duration = nextBoundary - current;
            var hours = (decimal)duration.TotalMinutes / 60m;

            totalPrice += hourlyRate
                          * hours
                          * GetMultiplier(current);

            current = nextBoundary;
        }

        return decimal.Round(
            totalPrice,
            2,
            MidpointRounding.AwayFromZero);
    }

    private static decimal GetMultiplier(DateTimeOffset time)
    {
        var currentTime = time.TimeOfDay;

        // Піковий тариф має пріоритет над стандартним,
        // оскільки проміжок 12:00–14:00 входить у стандартні години.
        if (currentTime >= TimeSpan.FromHours(12) &&
            currentTime < TimeSpan.FromHours(14))
        {
            return 1.15m;
        }

        if (currentTime >= TimeSpan.FromHours(6) &&
            currentTime < TimeSpan.FromHours(9))
        {
            return 0.90m;
        }

        if (currentTime >= TimeSpan.FromHours(18) &&
            currentTime < TimeSpan.FromHours(23))
        {
            return 0.80m;
        }

        return 1.00m;
    }

    private static DateTimeOffset GetNextBoundary(DateTimeOffset current)
    {
        var boundaries = new[]
        {
            TimeSpan.FromHours(6),
            TimeSpan.FromHours(9),
            TimeSpan.FromHours(12),
            TimeSpan.FromHours(14),
            TimeSpan.FromHours(18),
            TimeSpan.FromHours(23)
        };

        var nextBoundary = boundaries
            .FirstOrDefault(boundary => boundary > current.TimeOfDay);

        if (nextBoundary != default)
        {
            return new DateTimeOffset(
                current.Date + nextBoundary,
                current.Offset);
        }

        return new DateTimeOffset(
            current.Date.AddDays(1).AddHours(6),
            current.Offset);
    }
}