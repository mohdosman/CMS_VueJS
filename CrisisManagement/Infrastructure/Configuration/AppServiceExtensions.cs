using CrisisManagement.Features.Menus;
using CrisisManagement.Features.PublicFiles.Services;
using CrisisManagement.Features.Roles.Services;
using CrisisManagement.Features.Users.Services;
using CrisisManagement.Infrastructure.Identity;
using CrisisManagement.Infrastructure.Messaging.Email;

namespace CrisisManagement.Infrastructure.Configuration;

public static class AppServiceExtensions
{
    // Feature services and the small infrastructure services they use. Add a line here for each new feature.
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Sign-in helpers
        services.AddScoped<PasswordHistory>();
        services.Configure<MfaOptions>(configuration.GetSection("Mfa"));
        services.AddScoped<MfaService>();

        // Email (password reset links)
        services.Configure<SmtpOptions>(configuration.GetSection("Smtp"));
        services.AddScoped<IEmailSender, MailKitEmailSender>();

        // Features
        services.AddScoped<MenuService>();
        services.AddScoped<IPublicFilesService, PublicFilesService>();
        services.AddScoped<UserService>();
        services.AddScoped<RoleService>();
        services.AddScoped<UserDocumentService>();

        return services;
    }
}
