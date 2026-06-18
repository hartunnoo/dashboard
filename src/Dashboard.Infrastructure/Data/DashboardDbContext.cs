using Dashboard.Domain.Entities;
using Dashboard.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Dashboard.Infrastructure.Data;

public class DashboardDbContext : DbContext, IUnitOfWork
{
    public DashboardDbContext(DbContextOptions<DashboardDbContext> options) : base(options) { }
    public DbSet<DashboardNote> DashboardNotes => Set<DashboardNote>();
    public DbSet<Announcement> Announcements => Set<Announcement>();
    public DbSet<DashboardPreference> DashboardPreferences => Set<DashboardPreference>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(typeof(DashboardDbContext).Assembly);
    }
}
