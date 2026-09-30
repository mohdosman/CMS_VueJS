using CMS.Data;
using CMS.Data.Models.Identity;
using CMS.Infrastructure.Identity;
using CMS.Mvc.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace CMS.Mvc.Controllers;

[Authorize]
public class AccountController(
    SignInManager<ApplicationUser> signIn,
    UserManager<ApplicationUser> users,
    IUnitOfWork uow,
    PasswordHistory history,
    IConfiguration config,
    ILogger<AccountController> log) : Controller
{
    private const string InvalidLogin = "Invalid user ID or password.";

    [HttpGet, AllowAnonymous]
    public async Task<IActionResult> Login(string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        ViewData["ApplicationName"] = config["ApplicationName"];
        return View(await WithInfoAsync(new LoginViewModel()));
    }

    // The notices, support contacts and schema links beside the form. They are cosmetic, so a failure to
    // load them is logged and the page still lets people sign in.
    private async Task<LoginViewModel> WithInfoAsync(LoginViewModel model)
    {
        try
        {
            model.Info.Notices = (await uow.Notifications.GetLatestAsync(1))
                .Select(n => new NoticeItem(n.CreatedOn, n.Notification)).ToList();
            model.Info.Support = (await uow.Supports.GetWithContactsAsync(10))
                .Select(s => new SupportContact($"{s.Contact.FirstName} - {s.Contact.LastName}", s.Contact.Phone, s.Contact.EmailAddress)).ToList();

            var schema = config.GetSection("AssessmentSchema");
            if (!string.IsNullOrWhiteSpace(schema["SchemaUrl"]))
                model.Info.Schema = new SchemaLinks(schema["SchemaUrl"]!, schema["SampleXmlUrl"] ?? "", schema["LastUpdated"] ?? "");
        }
        catch (Exception ex)
        {
            log.LogError(ex, "Could not load the sign-in page notices and support contacts");
        }
        return model;
    }

    [HttpPost, AllowAnonymous, ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        ViewData["ApplicationName"] = config["ApplicationName"];
        if (!ModelState.IsValid) return View(await WithInfoAsync(model));

        // One message for every failure so the page never reveals which usernames exist.
        var user = await users.FindByNameAsync(model.UserName);
        if (user is null || user.IsADAccount || !user.IsActive)
        {
            log.LogWarning("Login rejected for {UserName}: unknown, AD or inactive account", model.UserName);
            ModelState.AddModelError("", InvalidLogin);
            return View(await WithInfoAsync(model));
        }

        var result = await signIn.PasswordSignInAsync(user, model.Password, isPersistent: false, lockoutOnFailure: true);
        if (!result.Succeeded)
        {
            log.LogWarning("Login failed for {UserName} (lockedOut={LockedOut})", model.UserName, result.IsLockedOut);
            ModelState.AddModelError("", InvalidLogin);
            return View(await WithInfoAsync(model));
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

    // Server-rendered, like SafetyNet. Same rules as the Blazor CMS: current password required,
    // no reuse of the last 12, and changing it clears the temporary-password flag.
    [HttpGet]
    public IActionResult Change()
    {
        ViewData["ApplicationName"] = config["ApplicationName"];
        return View(new ChangePasswordViewModel());
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Change(ChangePasswordViewModel model, CancellationToken ct)
    {
        ViewData["ApplicationName"] = config["ApplicationName"];
        if (!ModelState.IsValid) return View(model);

        var user = await users.GetUserAsync(User);
        if (user is null) return Challenge();

        if (user.IsADAccount)
        {
            ModelState.AddModelError("", "Active Directory accounts change their password through Microsoft.");
            return View(model);
        }
        if (model.Password == model.CurrentPassword)
        {
            ModelState.AddModelError(nameof(model.Password), "The new password must be different from the current password.");
            return View(model);
        }
        if (await history.IsReusedAsync(user, model.Password, ct))
        {
            ModelState.AddModelError(nameof(model.Password), $"You cannot reuse any of your last {PasswordHistory.Depth} passwords.");
            return View(model);
        }

        await using var tx = await uow.BeginTransactionAsync(ct);

        var changed = await users.ChangePasswordAsync(user, model.CurrentPassword, model.Password);
        if (!changed.Succeeded)
        {
            foreach (var e in changed.Errors)
            {
                // A field validation span shows only its first message, so the several rule failures of a
                // weak password go to the summary, which lists them all.
                if (e.Code == "PasswordMismatch") ModelState.AddModelError(nameof(model.CurrentPassword), "Current password is incorrect.");
                else ModelState.AddModelError("", e.Description);
            }
            return View(model);
        }

        history.Record(user);
        user.IsTemporaryPassword = false;
        var flags = await users.UpdateAsync(user);
        if (!flags.Succeeded)
        {
            ModelState.AddModelError("", "Your password could not be saved. Please try again.");
            return View(model);
        }
        await uow.SaveChangesAsync();
        await tx.CommitAsync(ct);

        // The password change rotates the security stamp; refresh so this session stays signed in.
        await signIn.RefreshSignInAsync(user);
        log.LogInformation("Password changed for user {UserId}", user.Id);

        TempData["StatusMessage"] = "Your password has been changed.";
        return RedirectToAction(nameof(Change));
    }
}
