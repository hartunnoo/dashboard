using System.Net.Http.Json;
using System.Text.Json;
using Dashboard.Application.DTOs;
using Dashboard.Application.Interfaces;
using Microsoft.Extensions.Configuration;

namespace Dashboard.Infrastructure.Services;

public class PrayerTimeClient : IPrayerTimeClient
{
    private readonly HttpClient _http;
    private static readonly JsonSerializerOptions JsonOpts = new() { PropertyNameCaseInsensitive = true };

    public PrayerTimeClient(HttpClient http, IConfiguration config)
    {
        _http = http;
        _http.BaseAddress = new Uri(config["PrayerTimeApi:BaseUrl"] ?? "https://lakastahsolat.com");
        _http.DefaultRequestHeaders.Add("X-Api-Key", config["PrayerTimeApi:ApiKey"] ?? "");
    }

    public async Task<string> GetHijriDateAsync(DateTime date, CancellationToken ct = default)
    {
        try
        {
            var resp = await _http.GetAsync($"/api/prayertime/today?date={date:yyyy-MM-dd}", ct);
            if (!resp.IsSuccessStatusCode) return FallbackHijri(date);
            var json = await resp.Content.ReadFromJsonAsync<JsonElement>(JsonOpts, ct);
            if (json.TryGetProperty("hijri", out var h)) return h.GetString() ?? FallbackHijri(date);
            return FallbackHijri(date);
        }
        catch { return FallbackHijri(date); }
    }

    public async Task<IReadOnlyList<PrayerSlotDto>> GetPrayerTimesAsync(CancellationToken ct = default)
    {
        try
        {
            var resp = await _http.GetAsync("/api/prayertime/today", ct);
            if (!resp.IsSuccessStatusCode) return Array.Empty<PrayerSlotDto>();
            var json = await resp.Content.ReadFromJsonAsync<JsonElement>(JsonOpts, ct);
            var slots = new[] { "Imsak", "Subuh", "Syuruk", "Zohor", "Asar", "Maghrib", "Isyak" };
            var result = new List<PrayerSlotDto>();
            foreach (var name in slots)
                if (json.TryGetProperty(name.ToLower(), out var t) && t.ValueKind != JsonValueKind.Null)
                    result.Add(new PrayerSlotDto(name, To12H(t.GetString() ?? "")));
            return result;
        }
        catch { return Array.Empty<PrayerSlotDto>(); }
    }

    public Task<string?> GetCurrentPrayerAsync(CancellationToken ct = default)
        => Task.FromResult<string?>(null);

    private static string To12H(string t)
    {
        if (string.IsNullOrEmpty(t)) return "";
        if (DateTime.TryParse(t, out var dt)) return dt.ToString("h:mm tt");
        return t;
    }

    private static string FallbackHijri(DateTime date)
    {
        var hc = new System.Globalization.HijriCalendar { HijriAdjustment = -2 };
        var months = new[] { "Muharram","Safar","Rabiulawal","Rabiulakhir","Jamadilawal","Jamadilakhir",
            "Rejab","Syaaban","Ramadhan","Syawal","Zulkaedah","Zulhijjah" };
        return $"{hc.GetDayOfMonth(date)} {months[hc.GetMonth(date)-1]} {hc.GetYear(date)}";
    }
}
