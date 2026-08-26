namespace Application.Abstractions.Reporting;

/// <summary>
/// Надає агреговані дані для бізнес-звітів.
/// </summary>
public interface IReportRepository
{
    Task<RevenueReportData> GetRevenueAsync(
        DateTimeOffset from,
        DateTimeOffset to,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<RoomUtilizationReportData>> GetRoomUtilizationAsync(
        DateTimeOffset from,
        DateTimeOffset to,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<PopularServiceReportData>> GetPopularServicesAsync(
        DateTimeOffset from,
        DateTimeOffset to,
        CancellationToken cancellationToken = default);
}

public sealed record RevenueReportData(
    int TotalBookings,
    decimal TotalRevenue);

public sealed record RoomUtilizationReportData(
    Guid ConferenceRoomId,
    string Name,
    int BookingsCount,
    decimal BookedHours);

public sealed record PopularServiceReportData(
    int ServiceId,
    string Name,
    int UsageCount);