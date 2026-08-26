using Application.Reports;
using Application.Reports.PopularServices;
using Application.Reports.Revenue;
using Application.Reports.RoomUtilization;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers;

[ApiController]
[Route("api/reports")]
public sealed class ReportsController : ControllerBase
{
    /// <summary>
    /// Повертає кількість бронювань і сумарний дохід за період.
    /// </summary>
    [HttpGet("revenue")]
    public async Task<ActionResult<RevenueReportResult>> GetRevenue(
        [FromQuery] DateTimeOffset from,
        [FromQuery] DateTimeOffset to,
        [FromServices] RevenueReportHandler handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(
            new ReportPeriod(from, to),
            cancellationToken);

        return Ok(result);
    }

    /// <summary>
    /// Повертає статистику використання конференц-залів.
    /// </summary>
    [HttpGet("room-utilization")]
    public async Task<ActionResult<IReadOnlyCollection<RoomUtilizationResult>>> GetRoomUtilization(
        [FromQuery] DateTimeOffset from,
        [FromQuery] DateTimeOffset to,
        [FromServices] RoomUtilizationHandler handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(
            new ReportPeriod(from, to),
            cancellationToken);

        return Ok(result);
    }

    /// <summary>
    /// Повертає рейтинг додаткових послуг за частотою використання.
    /// </summary>
    [HttpGet("popular-services")]
    public async Task<ActionResult<IReadOnlyCollection<PopularServiceResult>>> GetPopularServices(
        [FromQuery] DateTimeOffset from,
        [FromQuery] DateTimeOffset to,
        [FromServices] PopularServicesHandler handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(
            new ReportPeriod(from, to),
            cancellationToken);

        return Ok(result);
    }
}