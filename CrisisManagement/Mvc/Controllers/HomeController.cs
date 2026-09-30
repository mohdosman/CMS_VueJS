using CrisisManagement.Data.Models.Domain;
using CrisisManagement.Data.Models.Identity;
using CrisisManagement.Features.Menus;
using CrisisManagement.Mvc.ViewModels;
using CrisisManagement.Shared.Constants;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace CrisisManagement.Mvc.Controllers;

[Authorize]
public class HomeController(UserManager<ApplicationUser> users, MenuService menus, IConfiguration config) : Controller
{
    public async Task<IActionResult> Index([FromServices] IAntiforgery antiforgery)
    {
        var user = await users.GetUserAsync(User);
        if (user is null) return Challenge();

        return View(new HomeViewModel
        {
            ApplicationName = config["ApplicationName"] ?? "",
            UserName = user.UserName ?? "",
            FullName = user.FullName,
            IsAdmin = User.IsInRole(AppRoles.Admin),
            IsADAccount = user.IsADAccount,
            Roles = User.FindAll(User.Identities.First().RoleClaimType).Select(c => c.Value).ToList(),
            Permissions = User.FindAll(AppClaimTypes.Permission).Select(c => c.Value).Distinct().ToList(),
            Menu = await menus.GetMenuAsync(User),
            BaseUrl = Url.Content("~/").TrimEnd('/'),
            WebApiBaseUrl = Url.Content("~/api"),
            AntiforgeryToken = antiforgery.GetAndStoreTokens(HttpContext).RequestToken ?? ""
        });
    }
}
