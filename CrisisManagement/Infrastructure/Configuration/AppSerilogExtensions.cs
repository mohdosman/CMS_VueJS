using Serilog;

namespace CrisisManagement.Infrastructure.Configuration;

public static class AppSerilogExtensions
{
    // Replaces the host's logging with Serilog, configured from the "Serilog" section. FromLogContext keeps
    // ASP.NET's per-request scope, which puts RequestId on every line: the reference BaseApiController.Failure
    // hands the user for an unexpected error.
    public static WebApplicationBuilder AddAppSerilog(this WebApplicationBuilder builder)
    {
        builder.Host.UseSerilog((context, cfg) => cfg
            .ReadFrom.Configuration(context.Configuration)
            .Enrich.FromLogContext());
        return builder;
    }
}
