using CMS.Data.Models.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using QRCoder;

namespace CMS.Infrastructure.Identity;

// The "Mfa" section of appsettings.json. The issuer is baked into the authenticator entry at enrollment, so give each
// environment its own value or DEV/UAT/PROD entries look identical on the same phone.
public sealed class MfaOptions
{
    public string Issuer { get; set; } = "Crisis Management System";
}

// Names for the MFA rows Identity keeps in RBS_UserToken. Forced enrollment stores an authenticator key before the
// user has proved anything, so "a key exists" does not mean "enrolled": EnrolledOn is written only once a code has
// verified, and is what separates "must enroll now" from "ask for a code".
public static class MfaTokens
{
    public const string Provider = "[CrisisMgmt]";
    public const string EnrolledOn = "MfaEnrolledOn";
}

public sealed record MfaSetup(string SharedKey, string QrCodeDataUri);

// Authenticator-app MFA (port of the Blazor CMS ProfileService MFA methods). An administrator turns it on per account
// (TwoFactorEnabled, the "Require two-factor" switch); AD accounts are excluded because Entra does their MFA. There is
// no self-disable on purpose: only an administrator can lift the requirement (Reset MFA on the user screen).
public sealed class MfaService(UserManager<ApplicationUser> users, IOptions<MfaOptions> options)
{
    public bool IsRequired(ApplicationUser user) => !user.IsADAccount && user.TwoFactorEnabled;

    public async Task<bool> IsEnrolledAsync(ApplicationUser user) =>
        await users.GetAuthenticationTokenAsync(user, MfaTokens.Provider, MfaTokens.EnrolledOn) is not null;

    // A new key every time setup is opened, so an abandoned enrollment cannot be resumed by anyone who saw the
    // earlier QR code. Resetting also rotates the security stamp, so the caller refreshes the sign-in afterwards.
    public async Task<MfaSetup> BeginSetupAsync(ApplicationUser user)
    {
        await users.ResetAuthenticatorKeyAsync(user);
        return await CurrentSetupAsync(user);
    }

    // The key already issued, for showing the same QR code again after a wrong code.
    public async Task<MfaSetup> CurrentSetupAsync(ApplicationUser user)
    {
        var key = await users.GetAuthenticatorKeyAsync(user)
            ?? throw new InvalidOperationException($"Authenticator key was not stored for user {user.Id}");

        var issuer = options.Value.Issuer;
        var label = Uri.EscapeDataString($"{issuer}:{user.Email ?? user.UserName}");
        var uri = $"otpauth://totp/{label}?secret={key}&issuer={Uri.EscapeDataString(issuer)}&digits=6";

        using var generator = new QRCodeGenerator();
        using var data = generator.CreateQrCode(uri, QRCodeGenerator.ECCLevel.Q);
        var png = new PngByteQRCode(data).GetGraphic(6);

        return new MfaSetup(FormatKey(key), "data:image/png;base64," + Convert.ToBase64String(png));
    }

    // Enrollment is only ever completed by verifying a code, so EnrolledOn is written in exactly one place.
    public async Task<bool> EnableAsync(ApplicationUser user, string code)
    {
        if (!await VerifyAsync(user, code)) return false;

        var enabled = await users.SetTwoFactorEnabledAsync(user, true);
        if (!enabled.Succeeded) return false;

        await users.SetAuthenticationTokenAsync(user, MfaTokens.Provider, MfaTokens.EnrolledOn, DateTimeOffset.UtcNow.ToString("O"));
        return true;
    }

    public Task<bool> VerifyAsync(ApplicationUser user, string code) =>
        users.VerifyTwoFactorTokenAsync(user, users.Options.Tokens.AuthenticatorTokenProvider, Clean(code));

    // Administrator action: lifts the requirement and forgets the device, so the user can enroll a new one.
    public async Task ResetAsync(ApplicationUser user)
    {
        var result = await users.SetTwoFactorEnabledAsync(user, false);
        if (!result.Succeeded) throw new InvalidOperationException(string.Join(" ", result.Errors.Select(e => e.Description)));

        await users.ResetAuthenticatorKeyAsync(user);
        await users.RemoveAuthenticationTokenAsync(user, MfaTokens.Provider, MfaTokens.EnrolledOn);
    }

    public static string Clean(string? code) => (code ?? "").Replace(" ", "").Replace("-", "");

    public static bool IsSixDigits(string? code) => Clean(code) is { Length: 6 } c && c.All(char.IsAsciiDigit);

    // Authenticator apps accept the key with or without spaces; humans do better with groups of four.
    private static string FormatKey(string key) =>
        string.Join(' ', Enumerable.Range(0, (key.Length + 3) / 4).Select(i => key.Substring(i * 4, Math.Min(4, key.Length - i * 4))));
}
