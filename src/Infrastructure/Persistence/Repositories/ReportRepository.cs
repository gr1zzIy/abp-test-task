using Application.Abstractions.Reporting;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

internal sealed class ReportRepository : IReportRepository
{
    private readonly AppDbContext _dbContext;

    public ReportRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<RevenueReportData> GetRevenueAsync(
        DateTimeOffset from,
        DateTimeOffset to,
        CancellationToken cancellationToken = default)
    {
        var result = await _dbContext.Bookings
            .AsNoTracking()
            .Where(b => b.StartTime >= from && b.StartTime < to)
            .GroupBy(_ => 1)
            .Select(g => new RevenueReportData(
                g.Count(),
                g.Sum(b => b.TotalPrice)))
            .FirstOrDefaultAsync(cancellationToken);

        return result ?? new RevenueReportData(0, 0m);
    }

    public async Task<IReadOnlyCollection<RoomUtilizationReportData>> GetRoomUtilizationAsync(
        DateTimeOffset from,
        DateTimeOffset to,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Bookings
            .AsNoTracking()
            .Where(b => b.StartTime >= from && b.StartTime < to)
            .GroupBy(b => new
            {
                b.ConferenceRoomId,
                RoomName = b.ConferenceRoom.Name
            })
            .Select(g => new RoomUtilizationReportData(
                g.Key.ConferenceRoomId,
                g.Key.RoomName,
                g.Count(),
                decimal.Round(
                    g.Sum(b => (decimal)(b.EndTime - b.StartTime).TotalMinutes / 60m),
                    2)))
            .OrderByDescending(room => room.BookedHours)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<PopularServiceReportData>> GetPopularServicesAsync(
        DateTimeOffset from,
        DateTimeOffset to,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Services
            .AsNoTracking()
            .Select(s => new
            {
                s.Id,
                s.Name,
                UsageCount = s.Bookings.Count(b => b.StartTime >= from && b.StartTime < to)
            })
            .Where(s => s.UsageCount > 0)
            .OrderByDescending(s => s.UsageCount)
            .ThenBy(s => s.Name)
            .Select(s => new PopularServiceReportData(
                s.Id,
                s.Name,
                s.UsageCount))
            .ToListAsync(cancellationToken);
    }
}