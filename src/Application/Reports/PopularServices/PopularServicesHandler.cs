using Application.Abstractions.Reporting;
using FluentValidation;

namespace Application.Reports.PopularServices;

/// <summary>
/// Формує рейтинг додаткових послуг за кількістю
/// їх використань у бронюваннях.
/// </summary>
public sealed class PopularServicesHandler
{
    private readonly IReportRepository _reportRepository;
    private readonly IValidator<ReportPeriod> _validator;

    public PopularServicesHandler(
        IReportRepository reportRepository,
        IValidator<ReportPeriod> validator)
    {
        _reportRepository = reportRepository;
        _validator = validator;
    }

    public async Task<IReadOnlyCollection<PopularServiceResult>> HandleAsync(
        ReportPeriod period,
        CancellationToken cancellationToken = default)
    {
        await _validator.ValidateAndThrowAsync(
            period,
            cancellationToken);

        var data = await _reportRepository.GetPopularServicesAsync(
            period.From.ToUniversalTime(),
            period.To.ToUniversalTime(),
            cancellationToken);

        return data
            .Select(service => new PopularServiceResult(
                service.ServiceId,
                service.Name,
                service.UsageCount))
            .ToArray();
    }
}