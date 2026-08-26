namespace Application.Reports;

/// <summary>
/// Визначає часовий проміжок, за який формується бізнес-звіт.
/// </summary>
public sealed record ReportPeriod(
    DateTimeOffset From,
    DateTimeOffset To);