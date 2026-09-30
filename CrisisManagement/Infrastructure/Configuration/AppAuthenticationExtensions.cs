using CrisisManagement.Data.Context;
using CrisisManagement.Data.Models.Identity;
using CrisisManagement.Features.Users.ViewModels;
using CrisisManagement.Infrastructure.Identity;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;

namespace CrisisManagement.Infrastructure.Configuration;

public static class AppAuthenticationExtensions
{
    public static IServiceCollection AddAppAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<LegacyCryptoOptions>(configuration.GetSection("LegacyCrypto"));
        services.Configure<UserNamePolicyOptions>(configuration.GetSection("Identity:UserName"));
        services.AddScoped<IPasswordHasher<ApplicationUser>, HybridPasswordHasher>();

        services.AddIdentity<ApplicationUser, ApplicationRole>()
            .AddUserManager<HybridUserManager>()
            .AddClaimsPrincipalFactory<CmsClaimsPrincipalFactory>()
            .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders();   // password reset tokens

        services.Configure<IdentityOptions>(configuration.GetSection("Identity"));
        services.PostConfigure<IdentityOptions>(o => o.Password.RequiredLength = 8);   // fixed, same as the Blazor CMS

        // Password reset links: 2 hours unless Identity:Tokens:TokenLifespan says otherwise.
        services.Configure<DataProtectionTokenProviderOptions>(o => o.TokenLifespan = TimeSpan.FromHours(2));
        services.Configure<DataProtectionTokenProviderOptions>(configuration.GetSection("Identity:Tokens"));

        // "Sign in with Azure Active Directory": registered only when the AzureAd section is configured.
        services.AddAuthentication().AddEntraSignIn(configuration);

        services.ConfigureApplicationCookie(o =>
        {
            o.Cookie.Name = configuration["Session:CookieName"] ?? "cms.session";
            o.Cookie.HttpOnly = true;
            o.Cookie.SecurePolicy = CookieSecurePolicy.Always;
            o.ExpireTimeSpan = TimeSpan.FromMinutes(configuration.GetValue("Session:IdleMinutes", 60));
            o.SlidingExpiration = true;
            o.LoginPath = "/Account/Login";
            o.AccessDeniedPath = "/Account/Login";
            // The SPA calls /api with fetch: answer 401/403 instead of redirecting to the login page.
            o.Events.OnRedirectToLogin = ctx => ApiOrRedirect(ctx, StatusCodes.Status401Unauthorized);
            o.Events.OnRedirectToAccessDenied = ctx => ApiOrRedirect(ctx, StatusCodes.Status403Forbidden);
        });

        return services;
    }

    private static Task ApiOrRedirect(RedirectContext<CookieAuthenticationOptions> ctx, int status)
    {
        if (ctx.Request.Path.StartsWithSegments("/api")) ctx.Response.StatusCode = status;
        else ctx.Response.Redirect(ctx.RedirectUri);
        return Task.CompletedTask;
    }
}
