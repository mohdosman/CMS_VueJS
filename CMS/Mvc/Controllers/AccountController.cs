using CMS.Data;
using CMS.Data.Models.Identity;
using System.Text;
using CMS.Infrastructure.Identity;
using CMS.Infrastructure.Messaging.Email;
using CMS.Mvc.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;

namespace CMS.Mvc.Controllers;

[Authorize]
public class AccountController(
    SignInManager<ApplicationUser> signIn,
    UserManager<ApplicationUser> users,
    IUnitOfWork uow,
    PasswordHistory history,
    IConfiguration config,
    IEmailSender email,
    IOptions<DataProtectionTokenProviderOptions> resetTokens,
    ILogger<AccountController> log) : Controller
{
    private const string InvalidLogin = "Invalid user ID or password.";

    [HttpGet, AllowAnonymous]
    public async Task<IActionResult> Login(string? returnUrl = null, string? error = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        ViewData["ApplicationName"] = config["ApplicationName"];
        // Fixed messages by code, so nothing from the query string is ever shown.
        if (error == "entra") ModelState.AddModelError("", "Microsoft sign-in did not complete. Please try again.");
        else if (error == "entra-account") ModelState.AddModelError("", "Your Microsoft account is not linked to an active account in this application. Contact support.");
        return View(await WithInfoAsync(new LoginViewModel()));
    }

    // The notices, support contacts and schema links beside the form. They are cosmetic, so a failure to
    // load them is logged and the page still lets people sign in.
    private async Task<LoginViewModel> WithInfoAsync(LoginViewModel model)
    {
        model.Info.EntraEnabled = EntraSignIn.IsConfigured(config);
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

    // ---------------------------------------------------------------- forgot / reset password

    [HttpGet, AllowAnonymous]
    public IActionResult ForgotPassword() => View(new ForgotPasswordViewModel());

    // Always answers the same way for an unknown, inactive, locked or AD account, so the page cannot be used to
    // find out which user IDs exist. Only a failure of our own (mail relay down, template missing) is reported.
    [HttpPost, AllowAnonymous, ValidateAntiForgeryToken]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model, CancellationToken ct)
    {
        if (!ModelState.IsValid) return View(model);

        try
        {
            var input = model.UserNameOrEmail.Trim();
            var user = await users.FindByNameAsync(input) ?? await users.FindByEmailAsync(input);

            if (user is null || !await users.IsEmailConfirmedAsync(user))
                log.LogInformation("Forgot password: no confirmed account for the entered value");
            else if (!user.IsActive)
                log.LogInformation("Forgot password skipped for inactive user {UserId}", user.Id);
            else if (await users.IsLockedOutAsync(user))
                log.LogInformation("Forgot password skipped for locked out user {UserId}", user.Id);
            else if (user.IsADAccount)
                log.LogWarning("Forgot password used with an AD account {UserId}", user.Id);
            else
                await SendResetLinkAsync(user, ct);

            ViewData["Sent"] = true;
            return View(new ForgotPasswordViewModel());
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            log.LogError(ex, "Forgot password email failed");
            ModelState.AddModelError("", "The reset link could not be sent right now. Please try again later or contact support.");
            return View(model);
        }
    }

    private async Task SendResetLinkAsync(ApplicationUser user, CancellationToken ct)
    {
        var token = await users.GeneratePasswordResetTokenAsync(user);
        var tokenUrl = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));

        // A configured base URL keeps a forged Host header from pointing the emailed link at another site.
        var configured = config["App:BaseUrl"]?.Trim();
        var baseUrl = !string.IsNullOrWhiteSpace(configured)
            ? configured.TrimEnd('/')
            : $"{Request.Scheme}://{Request.Host}{Request.PathBase}";

        var body = await EmailTemplate.RenderAsync("password-reset", new Dictionary<string, string>
        {
            ["Link"] = $"{baseUrl}/Account/ResetPassword?userId={user.Id}&token={tokenUrl}",
            ["Expiry"] = Describe(resetTokens.Value.TokenLifespan)
        }, ct);

        await email.SendAsync(user.Email!, "Reset your password", body, ct);
        log.LogInformation("Password reset email sent for user {UserId}", user.Id);
    }

    // "2 hours", "30 minutes" for the email copy.
    private static string Describe(TimeSpan span)
    {
        var (n, unit) = span.TotalHours >= 1 ? (Math.Round(span.TotalHours, 1), "hour")
            : span.TotalMinutes >= 1 ? (Math.Round(span.TotalMinutes), "minute")
            : (Math.Round(span.TotalSeconds), "second");
        return $"{n:0.#} {unit}{(n == 1 ? "" : "s")}";
    }

    [HttpGet, AllowAnonymous]
    public IActionResult ResetPassword(int userId, string? token) =>
        userId <= 0 || string.IsNullOrWhiteSpace(token)
            ? View("ResetPasswordInvalid")
            : View(new ResetPasswordViewModel { UserId = userId, Token = token });

    [HttpPost, AllowAnonymous, ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model, CancellationToken ct)
    {
        if (!ModelState.IsValid) return View(model);

        var user = await users.FindByIdAsync(model.UserId.ToString());
        if (user is null || user.IsADAccount || !user.IsActive)
        {
            log.LogWarning("Reset password refused for user {UserId}: unknown, AD or inactive", model.UserId);
            return View("ResetPasswordInvalid");
        }

        string token;
        try
        {
            token = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(model.Token));
        }
        catch (FormatException)
        {
            log.LogWarning("Reset password: malformed token for user {UserId}", user.Id);
            return View("ResetPasswordInvalid");
        }

        if (await history.IsReusedAsync(user, model.Password, ct))
        {
            ModelState.AddModelError(nameof(model.Password), $"You cannot reuse any of your last {PasswordHistory.Depth} passwords.");
            return View(model);
        }

        await using var tx = await uow.BeginTransactionAsync(ct);

        var reset = await users.ResetPasswordAsync(user, token, model.Password);
        if (!reset.Succeeded)
        {
            // A bad or used token is not the user's fault to fix on this page: send them to request a new link.
            if (reset.Errors.Any(e => e.Code == "InvalidToken"))
            {
                log.LogWarning("Reset password: invalid or expired token for user {UserId}", user.Id);
                return View("ResetPasswordInvalid");
            }
            // Password rule failures go to the summary, which lists all of them.
            foreach (var e in reset.Errors) ModelState.AddModelError("", e.Description);
            return View(model);
        }

        history.Record(user);
        user.IsTemporaryPassword = false;
        user.LegacyPassword = "";   // the old-format hash must not outlive the reset
        var flags = await users.UpdateAsync(user);
        if (!flags.Succeeded)
        {
            ModelState.AddModelError("", "Your password could not be saved. Please try again.");
            return View(model);
        }
        await uow.SaveChangesAsync();
        await tx.CommitAsync(ct);

        log.LogInformation("Password reset completed for user {UserId}", user.Id);
        TempData["StatusMessage"] = "Your password has been reset. Sign in with your new password.";
        return RedirectToAction(nameof(Login));
    }
}
