using Dashboard.Domain.Entities;
using Dashboard.Domain.Interfaces;
using Dashboard.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Dashboard.Infrastructure.Repositories;

public class AnnouncementRepository(DashboardDbContext ctx) : IAnnouncementRepository
{
    public async Task<IReadOnlyList<Announcement>> GetActiveAsync(CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;
        return await ctx.Announcements
            .AsNoTracking()
            .Where(a => a.IsActive && (a.ExpiresAt == null || a.ExpiresAt > now))
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync(ct);
    }
    public async Task AddAsync(Announcement a, CancellationToken ct = default) => await ctx.Announcements.AddAsync(a, ct);
    public void Delete(Announcement a) => ctx.Announcements.Remove(a);
}
