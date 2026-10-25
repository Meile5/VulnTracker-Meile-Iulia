namespace VulnTracker.Application.DTOs;

public class SummaryResponse
{
    public int Open { get; init; }
    public int Overdue { get; init; }
    public Dictionary<string, int> OpenBySeverity { get; init; } = new();
    public Dictionary<string, int> ByStatus { get; init; } = new();
}
