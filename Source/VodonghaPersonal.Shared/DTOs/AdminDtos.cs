namespace VodonghaPersonal.Shared.DTOs;

// Dashboard
public record DashboardStatsDto(
    int TotalVisitors,
    int PageViews30d,
    int TotalBlogViews,
    int UnreadMessages,
    int UnreadChats,
    int SkillCount,
    int ProjectCount,
    int BlogCount,
    int ExpCount,
    int EduCount,
    List<string> SkillCatLabels,
    List<int> SkillCatData,
    List<string> BlogLabels,
    List<int> BlogData,
    List<string> TrendLabels,
    List<int> TrendData,
    List<RecentContactDto> RecentContacts
);

public record RecentContactDto(string Name, string Message, DateTime SentAt, bool IsRead);

// Analytics
public record AnalyticsDto(
    int Total,
    int TotalAll,
    List<TopItemDto> TopPages,
    List<TopItemDto> TopCountries,
    List<TopItemDto> TopReferrers,
    List<DailyViewDto> Daily
);

public record TopItemDto(string Label, int Count);
public record DailyViewDto(DateTime Date, int Count);

// Health
public record HealthSnapshotDto(
    DateTime Timestamp,
    long MemoryMb,
    double DbPingMs,
    int ThreadCount,
    bool DbHealthy
);

public record HealthDataDto(
    List<HealthSnapshotDto> Snapshots,
    HealthSnapshotDto? Latest,
    long UptimeSeconds,
    DateTime StartedAt,
    string AppVersion
);

// Dependencies
public record DependencyDto(
    string Name,
    string CurrentVersion,
    string? LatestVersion,
    string Type,
    string RegistryUrl,
    string? Notes,
    string Status
);

// Costs
public record FlyMachineDto(string Id, string State, string Region, string Size, int CpuCount, int MemoryMb);
public record FlyAppDto(
    string AppName,
    List<FlyMachineDto> Machines,
    double ComputePerHour,
    double ComputePerMonth24h,
    double Ipv4PerMonth,
    double FreeAllowance,
    double EstimatedBillable,
    double EstimatedMtdDollars
);
public record NeonProjectDto(
    string Name,
    string Plan,
    string Region,
    long StorageBytes,
    double StorageMb,
    double StorageGb,
    int PgVersion,
    double EstimatedMonthlyCost
);
public record CostSummaryDto(FlyAppDto? Fly, NeonProjectDto? Neon, DateTime FetchedAt);

// Settings
public record SettingsSaveRequest(Dictionary<string, string> Values);

// ApiKeys
public record ApiKeyDto(string Key, string Value);
public record ApiKeyDefinitionDto(string Key, string DisplayName, string Description, string Category, bool Sensitive);
public record ApiKeysPageDto(List<ApiKeyDefinitionDto> Definitions, Dictionary<string, string> DbValues, Dictionary<string, string> EnvValues);

// Orders
public record OrderUpdateRequest(List<int> Ids);

// Contacts
public record MarkReadRequest(int Id);

// Chat
public record ChatReplyRequest(string Content);
public record ChatSessionCreateRequest(string Name, string Phone, string Email);
public record ChatMessageRequest(string Content);
