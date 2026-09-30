using CrisisManagement.Infrastructure.Configuration;
using Serilog;
using Serilog.Events;

// Bootstrap logger: captures anything that fails before the host is built (a bad connection string, a missing
// setting), which would otherwise vanish because the real logger is configured from the host's configuration.
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .MinimumLevel.Override("System", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File(
        path: "Logs/CrisisManagementStartup-.log",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 7)
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting CrisisManagement...");

    var builder = WebApplication.CreateBuilder(args);

    builder.AddAppSerilog();
    builder.Services.AddInfrastructureServices(builder.Configuration);

    var app = builder.Build();

    app.UseAppMiddleware();

    app.Run();
}
catch (Exception ex) when (ex is not OperationCanceledException)
{
    Log.Fatal(ex, "CrisisManagement terminated unexpectedly.");
    throw;
}
finally
{
    Log.Information("CrisisManagement shut down.");
    Log.CloseAndFlush();
}
