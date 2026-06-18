using Dashboard.Application.DTOs;
using Dashboard.Application.Interfaces;
using Dapper;
using Microsoft.Extensions.Configuration;
using MySqlConnector;

namespace Dashboard.Infrastructure.Services;

public class ProjectSummaryService : IProjectSummaryClient
{
    private readonly string _connStr;

    public ProjectSummaryService(IConfiguration config)
    {
        _connStr = config.GetConnectionString("ProjectTrackerDb")
            ?? "Server=localhost;Port=3306;Database=project_tracker_projects;User=root;Password=Admin@123456;TreatTinyAsBoolean=true;CharSet=utf8mb4;Allow User Variables=true;";
    }

    public async Task<BoardKpiDto> GetKpisAsync(string userId, bool isPortfolioManager, CancellationToken ct = default)
    {
        using var conn = new MySqlConnection(_connStr);
        var active = await conn.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM Projects WHERE IsArchived=0");
        var risks = await conn.ExecuteScalarAsync<int>(
            "SELECT COUNT(*) FROM Projects WHERE IsArchived=0 AND Status IN (4,5)"); // Issues=4, Overdue=5
        return new BoardKpiDto(0, 0, active, risks);
    }

    public async Task<IReadOnlyList<BoardDeadlineDto>> GetDeadlinesAsync(string userId, bool isPortfolioManager, CancellationToken ct = default)
    {
        using var conn = new MySqlConnection(_connStr);
        var rows = await conn.QueryAsync(
            "SELECT Title as ProjectTitle, Owner, EndDate, DATEDIFF(EndDate, CURDATE()) as DaysUntilDeadline, ProgressPercentage as ProgressPercent FROM Projects WHERE IsArchived=0 AND EndDate IS NOT NULL ORDER BY EndDate LIMIT 5");
        return rows.Select(r => new BoardDeadlineDto(
            (string)r.ProjectTitle, (string)(r.Owner ?? "—"), (DateTime)r.EndDate,
            Convert.ToInt32(r.DaysUntilDeadline), Convert.ToInt32(r.ProgressPercent))).ToList().AsReadOnly();
    }

    public Task<IReadOnlyList<EnterpriseEventCardDto>> GetProjectBarsAsync(DateTime start, DateTime end, string userId, bool isPortfolioManager, CancellationToken ct = default)
        => Task.FromResult<IReadOnlyList<EnterpriseEventCardDto>>(Array.Empty<EnterpriseEventCardDto>());
}
