using CMS.Data;
using CMS.Data.Context;
using CMS.Data.Models.Domain;
using CMS.Data.Models.Identity;
using CMS.Features.Menus;
using CMS.Features.PublicFiles.Services;
using CMS.Features.Roles.Services;
using CMS.Features.Users.Services;
using CMS.Features.Users.ViewModels;
using CMS.Shared.Common;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using CMS.Infrastructure.Authorization;
using CMS.Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog((ctx, cfg) => cfg.ReadFrom.Configuration(ctx.Configuration));

var services = builder.Services;
var config = builder.Configuration;

// The database is the pre-existing CMS one; never run migrations against it.
services.AddDbContext<AppDbContext>(o => o.UseSqlServer(config.GetConnectionString("Default")));

services.Configure<LegacyCryptoOptions>(config.GetSection("LegacyCrypto"));
services.Configure<UserNamePolicyOptions>(config.GetSection("Identity:UserName"));
services.AddScoped<IPasswordHasher<ApplicationUser>, HybridPasswordHasher>();
services.AddIdentity<ApplicationUser, ApplicationRole>()
    .AddUserManager<HybridUserManager>()
    .AddClaimsPrincipalFactory<CmsClaimsPrincipalFactory>()
    .AddEntityFrameworkStores<AppDbContext>();

services.Configure<IdentityOptions>(config.GetSection("Identity"));
services.PostConfigure<IdentityOptions>(o => o.Password.RequiredLength = 8);   // fixed, same as the Blazor CMS

services.ConfigureApplicationCookie(o =>
{
    o.Cookie.Name = config["Session:CookieName"] ?? "cms.session";
    o.Cookie.HttpOnly = true;
    o.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    o.ExpireTimeSpan = TimeSpan.FromMinutes(config.GetValue("Session:IdleMinutes", 60));
    o.SlidingExpiration = true;
    o.LoginPath = "/Account/Login";
    o.AccessDeniedPath = "/Account/Login";
    // The SPA calls /api with fetch: answer 401/403 instead of redirecting to the login page.
    o.Events.OnRedirectToLogin = ctx => ApiOrRedirect(ctx, 401);
    o.Events.OnRedirectToAccessDenied = ctx => ApiOrRedirect(ctx, 403);
});

// Razor views live under /Mvc/Views (same layout as SafetyNet).
// Writes need the antiforgery token (SafetyNet: BaseController is AutoValidateAntiforgeryToken); the SPA
// sends it as X-XSRF-TOKEN.
services.AddAntiforgery(o => o.HeaderName = "X-XSRF-TOKEN");
services.AddControllersWithViews(o =>
    {
        o.Filters.Add(new Microsoft.AspNetCore.Mvc.AutoValidateAntiforgeryTokenAttribute());
    }).AddRazorOptions(o =>
{
    o.ViewLocationFormats.Clear();
    o.ViewLocationFormats.Add("/Mvc/Views/{1}/{0}.cshtml");
    o.ViewLocationFormats.Add("/Mvc/Views/Shared/{0}.cshtml");
});

// Permission policies ("users.view") are built on demand; see PermissionAuthorization.cs.
services.AddMemoryCache();
services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();
services.AddScoped<IAuthorizationHandler, PermissionHandler>();
services.AddScoped<PermissionCatalog>();
services.AddScoped<MenuService>();
services.AddHttpContextAccessor();
services.AddScoped<PasswordHistory>();
services.AddScoped<IUnitOfWork, HttpUnitOfWork>();
services.AddScoped<IPublicFilesService, PublicFilesService>();
services.AddScoped<UserService>();
services.AddScoped<RoleService>();
services.AddScoped<UserDocumentService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
    app.UseHsts();
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");

app.Run();

static Task ApiOrRedirect(RedirectContext<CookieAuthenticationOptions> ctx, int status)
{
    if (ctx.Request.Path.StartsWithSegments("/api")) ctx.Response.StatusCode = status;
    else ctx.Response.Redirect(ctx.RedirectUri);
    return Task.CompletedTask;
}
