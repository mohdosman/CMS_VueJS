using Microsoft.AspNetCore.Mvc;

namespace CrisisManagement.Infrastructure.Configuration;

public static class AppMvcExtensions
{
    public static IServiceCollection AddAppMvc(this IServiceCollection services)
    {
        // Every POST/PUT/DELETE needs the antiforgery token; the SPA sends it as X-XSRF-TOKEN (see http.js).
        services.AddAntiforgery(o => o.HeaderName = "X-XSRF-TOKEN");

        services.AddControllersWithViews(o => o.Filters.Add(new AutoValidateAntiforgeryTokenAttribute()))
            .AddRazorOptions(o =>
            {
                // Razor views live under /Mvc/Views instead of the default /Views (same layout as SafetyNet).
                o.ViewLocationFormats.Clear();
                o.ViewLocationFormats.Add("/Mvc/Views/{1}/{0}.cshtml");
                o.ViewLocationFormats.Add("/Mvc/Views/Shared/{0}.cshtml");
            });

        return services;
    }
}
