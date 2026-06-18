using Dashboard.Application.DTOs;

namespace Dashboard.Application.Interfaces;

public interface IProjectSummaryClient
{
    Task<BoardKpiDto> GetKpisAsync(string userId, bool isPortfolioManager, CancellationToken ct = default);
    Task<IReadOnlyList<BoardDeadlineDto>> GetDeadlinesAsync(string userId, bool isPortfolioManager, CancellationToken ct = default);
    Task<IReadOnlyList<EnterpriseEventCardDto>> GetProjectBarsAsync(DateTime start, DateTime end, string userId, bool isPortfolioManager, CancellationToken ct = default);
}
