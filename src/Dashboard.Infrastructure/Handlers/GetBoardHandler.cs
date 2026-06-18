using Dashboard.Application.DTOs;
using Dashboard.Application.Interfaces;
using Dashboard.Application.Queries.GetBoard;
using Dashboard.Domain.Constants;
using Dashboard.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Dashboard.Infrastructure.Handlers;

public class GetBoardHandler(
    ICalendarApiClient calClient,
    IProjectSummaryClient projClient,
    IPrayerTimeClient prayerClient,
    IDashboardNoteRepository noteRepo,
    ILogger<GetBoardHandler> logger)
    : IRequestHandler<GetBoardQuery, BoardViewModel>
{
    public async Task<BoardViewModel> Handle(GetBoardQuery q, CancellationToken ct)
    {
        var now = AppTime.NowInBrunei();
        var today = AppTime.TodayInBrunei();
        var weekEnd = today.AddDays(7);

        // Parallel fetch from all sources
        var calTask = calClient.GetEventsAsync(today, today.AddMonths(3), q.UserId, q.Roles, ct);
        var kpiTask = projClient.GetKpisAsync(q.UserId, q.IsPortfolioManager, ct);
        var deadTask = projClient.GetDeadlinesAsync(q.UserId, q.IsPortfolioManager, ct);
        var prayerTask = prayerClient.GetPrayerTimesAsync(ct);
        var hijriTask = prayerClient.GetHijriDateAsync(today, ct);
        var currentPrayerTask = prayerClient.GetCurrentPrayerAsync(ct);
        var notesTask = noteRepo.GetActiveAsync(ct);

        await Task.WhenAll(calTask, kpiTask, deadTask, prayerTask, hijriTask, currentPrayerTask, notesTask);

        var events = calTask.Result;
        var kpis = kpiTask.Result;
        var deadlines = deadTask.Result;
        var prayers = prayerTask.Result;
        var hijri = hijriTask.Result;
        var currentPrayer = currentPrayerTask.Result;
        var notes = notesTask.Result;

        // Filter today's events
        var todayIso = today.ToString("yyyy-MM-dd");
        var todayEvents = events
            .Where(e => e.Start.Date == today)
            .OrderBy(e => e.Start)
            .ToList();

        var currentEvent = todayEvents.FirstOrDefault(e => e.IsLive);
        var recentlyEnded = currentEvent == null
            ? todayEvents.FirstOrDefault(e => e.IsEnded && e.End.HasValue
                && (now - e.End.Value).TotalHours < 2)
            : null;

        var upcoming = events
            .Where(e => !e.IsLive && !e.IsEnded && e.Start > now)
            .OrderBy(e => e.Start).ToList();
        var nextEvent = upcoming.FirstOrDefault();

        var weekHighlights = events
            .Where(e => e.Start.Date > today && e.Start.Date <= weekEnd)
            .OrderBy(e => e.Start).Take(6).ToList();

        var noteDtos = notes.Select(n => new DashboardNoteDto(
            n.Id, n.Content, n.CreatedByUserId, n.CreatedByName,
            n.CreatedAt, n.StartsAt, n.ExpiresAt, n.IsActive)).ToList().AsReadOnly();

        var meetingsToday = todayEvents.Count;
        var tasksDue = 0; // from Project Tracker tasks

        return new BoardViewModel(
            now.ToString("hh:mm:ss tt"),
            today.ToString("dddd, d MMMM yyyy"),
            hijri,
            true,
            currentPrayer, null, null, null,
            prayers,
            currentEvent, recentlyEnded, nextEvent,
            todayEvents.AsReadOnly(),
            weekHighlights.AsReadOnly(),
            meetingsToday, tasksDue, kpis.ActiveProjects, kpis.Risks,
            Array.Empty<BoardAnnouncementDto>(),
            deadlines,
            noteDtos.Select(n => new BoardNoteDto(n.Content, n.CreatedByName)).ToList().AsReadOnly(),
            now
        );
    }
}
