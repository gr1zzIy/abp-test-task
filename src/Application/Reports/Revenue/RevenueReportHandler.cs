using Application.Abstractions.Reporting;
using FluentValidation;

namespace Application.Reports.Revenue;

/// <summary>
/// Формує звіт щодо кількості бронювань та отриманого доходу
/// за заданий часовий проміжок.
/// </summary>
public sealed class RevenueReportHandler
{
    private readonly IReportRepository _reportRepository;
    private readonly IValidator<ReportPeriod> _validator;

    public RevenueReportHandler(
        IReportRepository reportRepository,
        IValidator<ReportPeriod> validator)
    {
        _reportRepository = reportRepository;
        _validator = validator;
    }

    public async Task<RevenueReportResult> HandleAsync(
        ReportPeriod period,
        CancellationToken cancellationToken = default)
    {
        await _validator.ValidateAndThrowAsync(
            period,
            cancellationToken);

        // У БД час бронювань зберігається в UTC,
        // тому межі звітного періоду нормалізуємо перед виконанням запиту.
        var fromUtc = period.From.ToUniversalTime();
        var toUtc = period.To.ToUniversalTime();

        var data = await _reportRepository.GetRevenueAsync(
            fromUtc,
            toUtc,
            cancellationToken);

        return new RevenueReportResult(
            period.From,
            period.To,
            data.TotalBookings,
            data.TotalRevenue);
    }
}