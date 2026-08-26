using Application.Pricing;

namespace Application.Tests.Pricing;

public sealed class RentalPriceCalculatorTests
{
    private readonly RentalPriceCalculator _calculator = new();

    [Fact]
    public void Calculate_StandardHours_ReturnsBasePrice()
    {
        var startTime = CreateDateTime(10);
        var endTime = CreateDateTime(12);

        var result = _calculator.Calculate(
            2000m,
            startTime,
            endTime);

        Assert.Equal(4000m, result);
    }

    [Fact]
    public void Calculate_MorningHours_AppliesTenPercentDiscount()
    {
        var startTime = CreateDateTime(6);
        var endTime = CreateDateTime(9);

        var result = _calculator.Calculate(
            2000m,
            startTime,
            endTime);

        Assert.Equal(5400m, result);
    }

    [Fact]
    public void Calculate_PeakHours_AppliesFifteenPercentMarkup()
    {
        var startTime = CreateDateTime(12);
        var endTime = CreateDateTime(14);

        var result = _calculator.Calculate(
            2000m,
            startTime,
            endTime);

        Assert.Equal(4600m, result);
    }

    [Fact]
    public void Calculate_EveningHours_AppliesTwentyPercentDiscount()
    {
        var startTime = CreateDateTime(18);
        var endTime = CreateDateTime(20);

        var result = _calculator.Calculate(
            2000m,
            startTime,
            endTime);

        Assert.Equal(3200m, result);
    }

    [Fact]
    public void Calculate_WhenBookingCrossesPricingPeriods_CalculatesEachPeriodSeparately()
    {
        var startTime = CreateDateTime(11);
        var endTime = CreateDateTime(13);

        var result = _calculator.Calculate(
            2000m,
            startTime,
            endTime);

        // 11:00–12:00 = 2000, 12:00–13:00 = 2300.
        Assert.Equal(4300m, result);
    }

    [Fact]
    public void Calculate_MultiplePricingPeriods_ReturnsCorrectTotal()
    {
        var startTime = CreateDateTime(8);
        var endTime = CreateDateTime(15);

        var result = _calculator.Calculate(
            2000m,
            startTime,
            endTime);

        // 08:00–09:00 = 1800
        // 09:00–12:00 = 6000
        // 12:00–14:00 = 4600
        // 14:00–15:00 = 2000
        Assert.Equal(14400m, result);
    }

    [Fact]
    public void Calculate_PartialHour_CalculatesProportionally()
    {
        var startTime = CreateDateTime(10, 30);
        var endTime = CreateDateTime(11, 15);

        var result = _calculator.Calculate(
            2000m,
            startTime,
            endTime);

        Assert.Equal(1500m, result);
    }

    [Fact]
    public void Calculate_EndTimeBeforeStartTime_ThrowsArgumentException()
    {
        var startTime = CreateDateTime(12);
        var endTime = CreateDateTime(10);

        Assert.Throws<ArgumentException>(() =>
            _calculator.Calculate(
                2000m,
                startTime,
                endTime));
    }

    [Fact]
    public void Calculate_EqualStartAndEndTime_ThrowsArgumentException()
    {
        var time = CreateDateTime(10);

        Assert.Throws<ArgumentException>(() =>
            _calculator.Calculate(
                2000m,
                time,
                time));
    }

    private static DateTimeOffset CreateDateTime(
        int hour,
        int minute = 0)
    {
        return new DateTimeOffset(
            2026,
            9,
            1,
            hour,
            minute,
            0,
            TimeSpan.FromHours(3));
    }
    
    [Theory]
    [InlineData(9, 10, 2000)]
    [InlineData(14, 15, 2000)]
    [InlineData(17, 18, 2000)]
    public void Calculate_StandardHourBoundaries_UsesBaseRate(
        int startHour,
        int endHour,
        decimal expected)
    {
        var result = _calculator.Calculate(
            2000m,
            CreateDateTime(startHour),
            CreateDateTime(endHour));

        Assert.Equal(expected, result);
    }

    [Fact]
    public void Calculate_PeakToStandardBoundary_AppliesCorrectRates()
    {
        var result = _calculator.Calculate(
            2000m,
            CreateDateTime(13),
            CreateDateTime(15));

        // 13:00–14:00 = 2300, 14:00–15:00 = 2000.
        Assert.Equal(4300m, result);
    }
}