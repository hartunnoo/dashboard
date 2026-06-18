using Dashboard.Application.DTOs;

namespace Dashboard.Application.Interfaces;

public interface ICalendarApiClient
{
    Task<IReadOnlyList<EnterpriseEventCardDto>> GetEventsAsync(
        DateTime start, DateTime end, string userId, string roles,
        CancellationToken ct = default);
}
