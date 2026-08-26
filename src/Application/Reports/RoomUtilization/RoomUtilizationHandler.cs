using Application.Abstractions.Reporting;
using FluentValidation;

namespace Application.Reports.RoomUtilization;

/// <summary>
/// Формує статистику використання конференц-залів
/// за кількістю бронювань та сумарним заброньованим часом.
/// </summary>
public sealed class RoomUtilizationHandler
{
    private readonly IReportRepository _reportRepository;
    private readonly IValidator<ReportPeriod> _validator;

    public RoomUtilizationHandler(
        IReportRepository reportRepository,
        IValidator<ReportPeriod> validator)
    {
        _reportRepository = reportRepository;
        _validator = validator;
    }

    public async Task<IReadOnlyCollection<RoomUtilizationResult>> HandleAsync(
        ReportPeriod period,
        CancellationToken cancellationToken = default)
    {
        await _validator.ValidateAndThrowAsync(
            period,
            cancellationToken);

        var data = await _reportRepository.GetRoomUtilizationAsync(
            period.From.ToUniversalTime(),
            period.To.ToUniversalTime(),
            cancellationToken);

        return data
            .Select(room => new RoomUtilizationResult(
                room.ConferenceRoomId,
                room.Name,
                room.BookingsCount,
                room.BookedHours))
            .ToArray();
    }
}