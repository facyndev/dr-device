namespace Infrastructure.Jobs;

public sealed class UserCleanupOptions
{
    public const string SectionName = "UserCleanup";

    public int IntervalSeconds { get; set; } = 180;
    public int RetentionDays { get; set; } = 30;
    public int BatchSize { get; set; } = 100;
}
