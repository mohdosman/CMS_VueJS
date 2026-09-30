using CMS.Data.Models;
using CMS.Mvc.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace CMS.Mvc.Controllers;

[Authorize]
public class AccountController(
    SignInManager<ApplicationUser> signIn,
    UserManager<ApplicationUser> users,
    IConfiguration config,
    ILogger<AccountController> log) : Controller
{
    private const string InvalidLogin = "Invalid user ID or password.";

    [HttpGet, AllowAnonymous]
    public IActionResult Login(string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        ViewData["ApplicationName"] = config["ApplicationName"];
        return View(new LoginViewModel());
    }

    [HttpPost, AllowAnonymous, ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        ViewData["ApplicationName"] = config["ApplicationName"];
        if (!ModelState.IsValid) return View(model);

        // One message for every failure so the page never reveals which usernames exist.
        var user = await users.FindByNameAsync(model.UserName);
        if (user is null || user.IsADAccount || !user.IsActive)
        {
            log.LogWarning("Login rejected for {UserName}: unknown, AD or inactive account", model.UserName);
            ModelState.AddModelError("", InvalidLogin);
            return View(model);
        }

        var result = await signIn.PasswordSignInAsync(user, model.Password, isPersistent: false, lockoutOnFailure: true);
        if (!result.Succeeded)
        {
            log.LogWarning("Login failed for {UserName} (lockedOut={LockedOut})", model.UserName, result.IsLockedOut);
            ModelState.AddModelError("", InvalidLogin);
            return View(model);
        }

        user.LastLoginDate = DateTime.Now;
        await users.UpdateAsync(user);
        return Url.IsLocalUrl(returnUrl) ? LocalRedirect(returnUrl) : RedirectToAction("Index", "Home");
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await signIn.SignOutAsync();
        return RedirectToAction(nameof(Login));
    }
}
