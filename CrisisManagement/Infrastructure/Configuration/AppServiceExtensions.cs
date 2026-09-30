using CrisisManagement.Features.Assessments.Services;
using CrisisManagement.Features.Menus;
using CrisisManagement.Features.PublicFiles.Services;
using CrisisManagement.Features.Notifications.Services;
using CrisisManagement.Features.Providers.Services;
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
        services.AddScoped<ProviderScope>();

        // Sign-in helpers
        services.AddScoped<PasswordHistory>();
        services.Configure<MfaOptions>(configuration.GetSection("Mfa"));
        services.AddScoped<MfaService>();

        // Email (password reset links)
        services.Configure<SmtpOptions>(configuration.GetSection("Smtp"));
        services.AddScoped<IEmailSender, MailKitEmailSender>();

        // Features
        services.AddScoped<MenuService>();
        services.AddScoped<MenuAdminService>();
        services.AddScoped<IPublicFilesService, PublicFilesService>();
        services.AddScoped<UserService>();
        services.AddScoped<RoleService>();
        services.AddScoped<ProviderService>();
        services.AddScoped<NotificationService>();
        services.AddScoped<AssessmentSearchService>();
        services.AddScoped<AssessmentFileService>();
        services.AddScoped<UserDocumentService>();

        return services;
    }
}
