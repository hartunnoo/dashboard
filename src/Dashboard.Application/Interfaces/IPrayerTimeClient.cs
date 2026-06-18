using Dashboard.Application.DTOs;

namespace Dashboard.Application.Interfaces;

public interface IPrayerTimeClient
{
    Task<string> GetHijriDateAsync(DateTime date, CancellationToken ct = default);
    Task<IReadOnlyList<PrayerSlotDto>> GetPrayerTimesAsync(CancellationToken ct = default);
    Task<string?> GetCurrentPrayerAsync(CancellationToken ct = default);
}
