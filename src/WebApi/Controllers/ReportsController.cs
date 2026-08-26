using Application.Reports;
using Application.Reports.PopularServices;
using Application.Reports.Revenue;
using Application.Reports.RoomUtilization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using WebApi.Infrastructure;

namespace WebApi.Controllers;

[ApiController]
[Route("api/reports")]
public sealed class ReportsController : ControllerBase
{
    /// <summary>
    /// Формує звіт щодо доходу за заданий період.
    /// </summary>
    /// <remarks>
    /// До звіту потрапляють бронювання, час початку яких знаходиться
    /// у проміжку [from, to).
    /// </remarks>
    /// <param name="from">Початок звітного періоду.</param>
    /// <param name="to">Кінець звітного періоду, який не включається у вибірку.</param>
    [HttpGet("revenue")]
    [EnableRateLimiting(RateLimitingPolicies.Reports)]
    [ProducesResponseType<RevenueReportResult>(
        StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(
        StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
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
    /// Формує статистику використання конференц-залів.
    /// </summary>
    /// <remarks>
    /// Для кожного залу повертається кількість бронювань
    /// та сумарна кількість заброньованих годин за період.
    /// </remarks>
    /// <param name="from">Початок звітного періоду.</param>
    /// <param name="to">Кінець звітного періоду, який не включається у вибірку.</param>
    [HttpGet("room-utilization")]
    [EnableRateLimiting(RateLimitingPolicies.Reports)]
    [ProducesResponseType<IReadOnlyCollection<RoomUtilizationResult>>(
        StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(
        StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
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
    /// Формує рейтинг додаткових послуг за популярністю.
    /// </summary>
    /// <remarks>
    /// Послуги сортуються за кількістю їх використань
    /// у бронюваннях за заданий період.
    /// </remarks>
    /// <param name="from">Початок звітного періоду.</param>
    /// <param name="to">Кінець звітного періоду, який не включається у вибірку.</param>
    [HttpGet("popular-services")]
    [EnableRateLimiting(RateLimitingPolicies.Reports)]
    [ProducesResponseType<IReadOnlyCollection<PopularServiceResult>>(
        StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(
        StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
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