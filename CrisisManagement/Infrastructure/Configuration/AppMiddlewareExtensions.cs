using CrisisManagement.Infrastructure.Identity;

namespace CrisisManagement.Infrastructure.Configuration;

public static class AppMiddlewareExtensions
{
    // The request pipeline. Order matters: authentication before the required-action gate, the gate before authorization.
    public static WebApplication UseAppMiddleware(this WebApplication app)
    {
        if (!app.Environment.IsDevelopment())
            app.UseHsts();

        app.UseHttpsRedirection();
        app.UseStaticFiles();
        app.UseRouting();
        app.UseAuthentication();
        app.UseMiddleware<RequiredAccountActionMiddleware>();   // pins users who owe a password change or MFA setup
        app.UseAuthorization();

        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Home}/{action=Index}/{id?}");

        return app;
    }
}
