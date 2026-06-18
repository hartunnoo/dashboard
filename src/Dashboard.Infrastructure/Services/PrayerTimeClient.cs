using System.Net.Http.Json;
using System.Text.Json;
using Dashboard.Application.DTOs;
using Dashboard.Application.Interfaces;
using Dashboard.Domain.Constants;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Dashboard.Infrastructure.Services;

public class PrayerTimeClient : IPrayerTimeClient
{
    private readonly HttpClient _http;
    private readonly ILogger<PrayerTimeClient> _logger;
    private static readonly JsonSerializerOptions JsonOpts = new() { PropertyNameCaseInsensitive = true };

    public PrayerTimeClient(HttpClient http, IConfiguration config, ILogger<PrayerTimeClient> logger)
    {
        _http = http;
        _logger = logger;
        _http.BaseAddress = new Uri(config["PrayerTimeApi:BaseUrl"] ?? "https://lakastahsolat.com");
        _http.DefaultRequestHeaders.Add("X-Api-Key", config["PrayerTimeApi:ApiKey"] ?? "");
        _http.Timeout = TimeSpan.FromSeconds(10);
    }

    public async Task<string> GetHijriDateAsync(DateTime date, CancellationToken ct = default)
    {
        try
        {
            var today = await GetTodayAsync(ct);
            if (today != null && today.Value.TryGetProperty("Hijri", out var h))
                return h.GetString() ?? FallbackHijri(date);
        }
        catch (Exception ex) { _logger.LogWarning(ex, "Hijri fetch failed"); }
        return FallbackHijri(date);
    }

    public async Task<IReadOnlyList<PrayerSlotDto>> GetPrayerTimesAsync(CancellationToken ct = default)
    {
        try
        {
            var today = await GetTodayAsync(ct);
            if (today == null) return Array.Empty<PrayerSlotDto>();
            var t = today.Value;

            var names = new[] { "Imsak", "Subuh", "Syuruk", "Zohor", "Asar", "Maghrib", "Isyak" };
            var result = new List<PrayerSlotDto>();
            foreach (var name in names)
            {
                if (t.TryGetProperty(name, out var tv) && tv.ValueKind != JsonValueKind.Null)
                {
                    var time = tv.GetString() ?? "";
                    result.Add(new PrayerSlotDto(name, To12H(time)));
                }
            }
            return result;
        }
        catch (Exception ex) { _logger.LogWarning(ex, "Prayer times fetch failed"); }
        return Array.Empty<PrayerSlotDto>();
    }

    public async Task<string?> GetCurrentPrayerAsync(CancellationToken ct = default)
    {
        try
        {
            var today = await GetTodayAsync(ct);
            if (today == null) return null;
            var t = today.Value;

            var now = AppTime.NowInBrunei().TimeOfDay;
            var slots = new[] { ("Imsak","Imsak"), ("Subuh","Subuh"), ("Syuruk","Syuruk"),
                ("Zohor","Zohor"), ("Asar","Asar"), ("Maghrib","Maghrib"), ("Isyak","Isyak") };

            for (int i = slots.Length - 1; i >= 0; i--)
            {
                if (t.TryGetProperty(slots[i].Item2, out var tv) && tv.ValueKind != JsonValueKind.Null)
                {
                    if (TimeSpan.TryParse(tv.GetString(), out var time) && now >= time)
                        return slots[i].Item1;
                }
            }
        }
        catch { }
        return null;
    }

    private async Task<JsonElement?> GetTodayAsync(CancellationToken ct)
    {
        var resp = await _http.GetAsync("/api/PrayerTimeApi", ct);
        if (!resp.IsSuccessStatusCode) return null;

        var all = await resp.Content.ReadFromJsonAsync<List<JsonElement>>(JsonOpts, ct);
        if (all == null) return null;

        foreach (var p in all)
        {
            if (p.TryGetProperty("IsToday", out var isToday) && isToday.GetBoolean())
                return p;
        }
        return null;
    }

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
