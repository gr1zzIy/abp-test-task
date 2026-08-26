namespace Application.Reports.PopularServices;

public sealed record PopularServiceResult(
    int ServiceId,
    string Name,
    int UsageCount);