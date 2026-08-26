namespace Application.Reports.Revenue;

public sealed record RevenueReportResult(
    DateTimeOffset From,
    DateTimeOffset To,
    int TotalBookings,
    decimal TotalRevenue);