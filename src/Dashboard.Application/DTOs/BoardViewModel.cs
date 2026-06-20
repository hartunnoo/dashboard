namespace Dashboard.Application.DTOs;

public sealed record BoardViewModel(
    string CurrentTime,
    string CurrentDate,
    string HijriDate,
    bool IsOnline,
    string? CurrentPrayer,
    string? NextPrayer,
    string? NextPrayerTime,
    string? PrayerLocation,
    IReadOnlyList<PrayerSlotDto> PrayerTimes,
    EnterpriseEventCardDto? CurrentEvent,
    EnterpriseEventCardDto? RecentlyEndedEvent,
    EnterpriseEventCardDto? NextEvent,
    IReadOnlyList<EnterpriseEventCardDto> TodayEvents,
    IReadOnlyList<EnterpriseEventCardDto> WeekHighlights,
    int MeetingsToday,
    int TasksDueToday,
    int ActiveProjects,
    int RisksRequiringAttention,
    IReadOnlyList<BoardAnnouncementDto> Announcements,
    IReadOnlyList<BoardDeadlineDto> UpcomingDeadlines,
    IReadOnlyList<BoardNoteDto> Notes,
    DateTime LastUpdated,
    int AutoRefreshSeconds = 60,
    bool IsPortfolioManager = false,
    string UserDisplayName = ""
);

public sealed record BoardAnnouncementDto(string Title, string Content, string Severity, string? Icon);
public sealed record BoardNoteDto(string Content, string AuthorName);

public sealed record PrayerSlotDto(string Name, string Time);
public sealed record EnterpriseEventCardDto(
    string Id, string Title, string CategoryName, string? CategoryColor,
    DateTime Start, DateTime? End, string StartTime, string? EndTime, bool AllDay,
    string? Location, string? Duration, string? Stakeholder, string? Countdown,
    bool IsLive, bool IsEnded, string? EndedAgo, int ParticipantCount = 0);
public sealed record BoardKpiDto(int MeetingsToday, int TasksDue, int ActiveProjects, int Risks);
public sealed record BoardDeadlineDto(string ProjectTitle, string Owner, DateTime EndDate, int DaysUntilDeadline, int ProgressPercent);

