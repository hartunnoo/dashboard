using Serilog;
using Dashboard.Application;
using Dashboard.Infrastructure;

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var appDir = OperatingSystem.IsWindows()
    ? @"C:\Users\hartu\apps\dashboard"
    : "/opt/dashboard";
Directory.CreateDirectory(Path.Combine(appDir, "logs"));

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .WriteTo.File(Path.Combine(appDir, "logs", "dashboard-api-.log"),
        rollingInterval: RollingInterval.Day, retainedFileCountLimit: 7)
    .CreateLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);
    builder.Host.UseSerilog();
    if (OperatingSystem.IsWindows()) builder.Host.UseWindowsService();

    builder.Services.AddControllersWithViews().AddRazorRuntimeCompilation();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();
    builder.Services.AddSignalR();

    builder.Services.AddApplicationServices();
    builder.Services.AddInfrastructureServices(builder.Configuration);

    var app = builder.Build();

    app.UseDefaultFiles();
    app.UseStaticFiles();

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseRouting();

    // API key auth middleware
    app.Use(async (ctx, next) =>
    {
        var path = ctx.Request.Path.Value ?? "";
        if (path == "/" || path == "/health" || path.StartsWith("/swagger") || path.StartsWith("/hubs/"))
        { await next(); return; }
        var key = ctx.Request.Headers["X-Api-Key"].FirstOrDefault();
        var expected = app.Configuration.GetValue<string>("ApiKey") ?? "dashboard-api-key-2026";
        if (key != expected) { ctx.Response.StatusCode = 401; await ctx.Response.WriteAsync("Unauthorized"); return; }
        await next();
    });

    app.MapControllers();
    app.MapGet("/health", () => Results.Ok(new { status = "healthy", service = "DashboardService" }));

    await app.RunAsync();
}
catch (Exception ex) { Log.Fatal(ex, "Dashboard Service terminated"); }
finally { await Log.CloseAndFlushAsync(); }
