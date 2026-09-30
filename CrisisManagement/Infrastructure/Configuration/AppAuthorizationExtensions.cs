using CrisisManagement.Infrastructure.Authorization;
using Microsoft.AspNetCore.Authorization;

namespace CrisisManagement.Infrastructure.Configuration;

public static class AppAuthorizationExtensions
{
    // Permission policies ("users.view") are built on demand from the RBS_Permission catalog; see PermissionAuthorization.cs.
    public static IServiceCollection AddAppAuthorization(this IServiceCollection services)
    {
        services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();
        services.AddScoped<IAuthorizationHandler, PermissionHandler>();
        services.AddScoped<PermissionCatalog>();
        return services;
    }
}
