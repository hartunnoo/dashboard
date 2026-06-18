using Dashboard.Domain.Entities;

namespace Dashboard.Domain.Interfaces;

public interface IAnnouncementRepository
{
    Task<IReadOnlyList<Announcement>> GetActiveAsync(CancellationToken ct = default);
    Task AddAsync(Announcement a, CancellationToken ct = default);
    void Delete(Announcement a);
}
