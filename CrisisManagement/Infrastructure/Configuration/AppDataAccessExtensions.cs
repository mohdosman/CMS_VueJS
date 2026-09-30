using CrisisManagement.Data;
using CrisisManagement.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace CrisisManagement.Infrastructure.Configuration;

public static class AppDataAccessExtensions
{
    public static IServiceCollection AddAppDataAccess(this IServiceCollection services, IConfiguration configuration)
    {
        // The database is the pre-existing CrisisMgmt one; never run migrations against it.
        services.AddDbContext<AppDbContext>(o => o.UseSqlServer(configuration.GetConnectionString("Default")));

        // Services reach data only through the unit of work; HttpUnitOfWork stamps the audit columns with the signed-in user.
        services.AddScoped<IUnitOfWork, HttpUnitOfWork>();

        return services;
    }
}
