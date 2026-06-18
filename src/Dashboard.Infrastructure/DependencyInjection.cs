using Dashboard.Application.Interfaces;
using Dashboard.Domain.Interfaces;
using Dashboard.Infrastructure.Data;
using Dashboard.Infrastructure.Repositories;
using Dashboard.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Dashboard.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services, IConfiguration configuration)
    {
        var connStr = configuration.GetConnectionString("DashboardDb");
        if (string.IsNullOrEmpty(connStr))
            services.AddDbContext<DashboardDbContext>(o => o.UseInMemoryDatabase("DashboardDev"));
        else
            services.AddDbContext<DashboardDbContext>(o => o.UseNpgsql(connStr));

        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<DashboardDbContext>());
        services.AddScoped<IDashboardNoteRepository, DashboardNoteRepository>();
        services.AddScoped<IAnnouncementRepository, AnnouncementRepository>();

        // API clients — typed HttpClients
        services.AddHttpClient<ICalendarApiClient, CalendarApiClient>(client =>
        {
            client.BaseAddress = new Uri(configuration["CalendarService:BaseUrl"] ?? "http://localhost:5200");
            client.Timeout = TimeSpan.FromSeconds(15);
        });

        services.AddHttpClient<IPrayerTimeClient, PrayerTimeClient>(client =>
        {
            client.Timeout = TimeSpan.FromSeconds(10);
        });

        // Project data — reads MySQL directly (same DB as Project Tracker)
        services.AddSingleton<IProjectSummaryClient, ProjectSummaryService>();

        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly));
        return services;
    }
}
