namespace CrisisManagement.Infrastructure.Configuration;

public static class ServiceCollectionExtensions
{
    // Everything Program.cs would otherwise register inline. Nothing here depends on a later registration.
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddAppDataAccess(configuration);
        services.AddAppAuthentication(configuration);
        services.AddAppAuthorization();
        services.AddAppMvc();
        services.AddMemoryCache();
        services.AddHttpContextAccessor();
        services.AddApplicationServices(configuration);
        return services;
    }
}
