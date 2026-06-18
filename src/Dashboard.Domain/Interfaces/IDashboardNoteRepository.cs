using Dashboard.Domain.Entities;

namespace Dashboard.Domain.Interfaces;

public interface IDashboardNoteRepository
{
    Task<IReadOnlyList<DashboardNote>> GetActiveAsync(CancellationToken ct = default);
    Task<DashboardNote?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task AddAsync(DashboardNote note, CancellationToken ct = default);
    void Delete(DashboardNote note);
}
