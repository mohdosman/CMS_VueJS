using CMS.Data.Models.Identity;
using CMS.Infrastructure.Identity;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace CMS.Mvc.Controllers;

// Microsoft Entra ID sign-in for Active Directory accounts, same shape as SafetyNet AzureAuthController:
// SignIn sends the browser to Microsoft, Callback receives the result. See EntraSignIn for the setup.
[AllowAnonymous]
public class AzureAuthController(
    SignInManager<ApplicationUser> signIn,
    UserManager<ApplicationUser> users,
    IConfiguration config,
    ILogger<AzureAuthController> log) : Controller
{
    [HttpGet]
    public IActionResult SignIn(string? returnUrl = null)
    {
        if (!EntraSignIn.IsConfigured(config)) return NotFound();

        var properties = signIn.ConfigureExternalAuthenticationProperties(
            EntraSignIn.Scheme, Url.Action(nameof(Callback), "AzureAuth", new { returnUrl }));
        return Challenge(properties, EntraSignIn.Scheme);
    }

    // Microsoft has authenticated the person; this decides whether that person may use this application. They
    // must match an existing, active, unlocked account that is marked as an AD account. Local accounts are
    // never signed in this way, so a Microsoft user named like a local account cannot take it over.
    [HttpGet]
    public async Task<IActionResult> Callback(string? returnUrl = null, string? remoteError = null)
    {
        if (!string.IsNullOrEmpty(remoteError))
        {
            log.LogWarning("Microsoft sign-in returned an error: {Error}", remoteError);
            return ToLogin("entra");
        }

        var info = await signIn.GetExternalLoginInfoAsync();
        if (info is null)
        {
            log.LogWarning("Microsoft sign-in callback without external login info");
            return ToLogin("entra");
        }

        // "ci01578@tn.gov" -> "ci01578", the user ID of the AD account (same rule as the Blazor CMS and SafetyNet).
        var name = info.Principal.FindFirst("preferred_username")?.Value ?? info.Principal.FindFirst("upn")?.Value;
        var userName = name?.Split('@')[0];
        var user = string.IsNullOrWhiteSpace(userName) ? null : await users.FindByNameAsync(userName);

        if (user is null || !user.IsADAccount || !user.IsActive || await users.IsLockedOutAsync(user))
        {
            log.LogWarning("Microsoft sign-in refused for {UserName}: no active linked AD account", userName);
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);
            return ToLogin("entra-account");
        }

        await signIn.SignInAsync(user, isPersistent: false);
        await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

        user.LastLoginDate = DateTime.Now;
        await users.UpdateAsync(user);
        log.LogInformation("User {UserName} signed in with Microsoft Entra ID", user.UserName);

        return Url.IsLocalUrl(returnUrl) ? LocalRedirect(returnUrl) : RedirectToAction("Index", "Home");
    }

    private IActionResult ToLogin(string error) => RedirectToAction("Login", "Account", new { error });
}
