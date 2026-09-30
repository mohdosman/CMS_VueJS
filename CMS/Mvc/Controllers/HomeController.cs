using CMS.Data.Models.Domain;
using CMS.Data.Models.Identity;
using CMS.Features.Menus;
using CMS.Mvc.ViewModels;
using CMS.Shared.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace CMS.Mvc.Controllers;

[Authorize]
public class HomeController(UserManager<ApplicationUser> users, MenuService menus, IConfiguration config) : Controller
{
    public async Task<IActionResult> Index()
    {
        var user = await users.GetUserAsync(User);
        if (user is null) return Challenge();

        return View(new HomeViewModel
        {
            ApplicationName = config["ApplicationName"] ?? "",
            UserName = user.UserName ?? "",
            FullName = user.FullName,
            IsAdmin = User.IsInRole(AppRoles.Admin),
            Roles = User.FindAll(User.Identities.First().RoleClaimType).Select(c => c.Value).ToList(),
            Permissions = User.FindAll(AppClaimTypes.Permission).Select(c => c.Value).Distinct().ToList(),
            Menu = await menus.GetMenuAsync(User)
        });
    }
}
