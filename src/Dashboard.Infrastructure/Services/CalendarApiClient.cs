using System.Net.Http.Json;
using System.Text.Json;
using Dashboard.Application.DTOs;
using Dashboard.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Dashboard.Infrastructure.Services;

public class CalendarApiClient : ICalendarApiClient
{
    private readonly HttpClient _http;
    private readonly string _apiKey;
    private static readonly JsonSerializerOptions JsonOpts = new() { PropertyNameCaseInsensitive = true };

    public CalendarApiClient(HttpClient http, IConfiguration config)
    {
        _http = http;
        _apiKey = config["CalendarService:ApiKey"] ?? "calendar-service-api-key-2026";
    }

    public async Task<IReadOnlyList<EnterpriseEventCardDto>> GetEventsAsync(
        DateTime start, DateTime end, string userId, string roles, CancellationToken ct = default)
    {
        var url = $"/api/events?start={start:yyyy-MM-dd}&end={end:yyyy-MM-dd}";
        var req = new HttpRequestMessage(HttpMethod.Get, url);
        req.Headers.Add("X-Api-Key", _apiKey);
        req.Headers.Add("X-User-Id", userId);
        if (!string.IsNullOrEmpty(roles)) req.Headers.Add("X-User-Roles", roles);

        var resp = await _http.SendAsync(req, ct);
        if (!resp.IsSuccessStatusCode) return Array.Empty<EnterpriseEventCardDto>();

        var items = await resp.Content.ReadFromJsonAsync<List<JsonElement>>(JsonOpts, ct);
        var list = items?.Select(MapEvent).ToList() ?? new List<EnterpriseEventCardDto>();
        return list.AsReadOnly();
    }

    private static EnterpriseEventCardDto MapEvent(JsonElement e)
    {
        var start = e.TryGetProperty("start", out var s) ? ParseDt(s.GetString()) : DateTime.MinValue;
        var end = e.TryGetProperty("end", out var en) && en.ValueKind != JsonValueKind.Null ? ParseDt(en.GetString()) : (DateTime?)null;
        var allDay = e.TryGetProperty("allDay", out var ad) && ad.GetBoolean();
        var now = DateTime.UtcNow.AddHours(8);
        var isLive = allDay ? start.Date == now.Date : start <= now && (end.HasValue && end.Value > now);
        var isEnded = end.HasValue && end.Value < now;

        string? countdown = null;
        if (!isLive && !isEnded && start > now)
        {
            var diff = start - now;
            countdown = diff.TotalDays >= 1 ? $"in {diff.Days}d {diff.Hours}h" : $"in {diff.Hours}h {diff.Minutes}m";
        }

        return new EnterpriseEventCardDto(
            e.TryGetProperty("id", out var id) ? id.GetString() ?? "" : "",
            e.TryGetProperty("title", out var t) ? t.GetString() ?? "" : "",
            e.TryGetProperty("extendedProps", out var ep) && ep.TryGetProperty("categoryName", out var cn) ? cn.GetString() ?? "" : "",
            e.TryGetProperty("color", out var c) ? c.GetString() : null,
            start, end,
            allDay ? "All Day" : start.ToString("h:mm tt"),
            end?.ToString("h:mm tt"), allDay,
            null, null,
            ep.TryGetProperty("stakeholder", out var sh) ? sh.GetString() : null,
            countdown,
            isLive, isEnded, null);
    }

    private static DateTime ParseDt(string? s)
    {
        if (string.IsNullOrEmpty(s)) return DateTime.MinValue;
        if (s.Length == 10) return DateTime.Parse(s + "T00:00:00");
        return DateTime.TryParse(s, out var dt) ? dt : DateTime.MinValue;
    }
}
