using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using CMS.Data;
using CMS.Data.Repositories.Interfaces;
using CMS.Data.Models.Domain;
using CMS.Data.Models.Identity;
using CMS.Features.Users.Mapping;
using CMS.Features.Users.Repositories;
using CMS.Features.Users.ViewModels;
using CMS.Infrastructure.Identity;
using CMS.Shared.Common;
using CMS.Shared.Constants;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace CMS.Features.Users.Services;

// Port of the Blazor CMS UserService (search, read, create, update, set password, delete). Data access goes
// through the unit of work; UserManager is used only for Identity operations (hashing, create, delete). Non-admins are scoped two ways:
//  - roles: only users/roles matching a "users.role.<slug>" permission claim
//  - providers: only users sharing a provider with the provider_ids claim of the caller
// MFA reset and agreement documents are not ported yet.
public sealed class UserService(
    IUnitOfWork uow,
    UserManager<ApplicationUser> users,
    IHttpContextAccessor http,
    IOptions<IdentityOptions> identityOptions,
    IOptions<UserNamePolicyOptions> userNamePolicy,
    PasswordHistory history)
{
    private const string RolePrefix = "users.role.";
    private const int MaxNameLength = 256, MaxEmailLength = 100, MaxLocalUserNameLength = 50, MaxPhoneLength = 32;

    private ClaimsPrincipal Principal => http.HttpContext!.User;
    private bool IsAdmin => Principal.IsInRole(AppRoles.Admin);

    // ---------------------------------------------------------------- read

    public async Task<PagedResult<UserListItem>> SearchAsync(UserSearchRequest req, CancellationToken ct) =>
        await uow.Users.SearchAsync(req, new UserSearchScope(IsAdmin, await GetScopeAsync(), ProviderIdsClaim()));

    // Detail null + Allowed true = not found; Allowed false = a target role is outside the caller scope.
    public async Task<(UserDetail? Detail, bool Allowed)> GetAsync(Guid userKey, CancellationToken ct)
    {
        var u = await uow.Users.GetByKeyNoTrackingAsync(userKey);
        if (u is null) return (null, true);

        var roles = await uow.UserRoles.GetRolesForUserAsync(u.Id);
        if (!IsAdmin && !RolesWithinScope(roles.Select(r => r.Name))) return (null, false);

        var providers = await uow.ProviderUsers.GetProvidersForUserAsync(u.Id);
        var lastLogon = await uow.Logons.GetLastLogOnAsync(u.Id);
        return (u.ToDetail(roles, providers, lastLogon), true);
    }

    public async Task<List<LookupItem>> GetRolesAsync(CancellationToken ct) =>
        (await uow.Roles.GetIdNamesAsync(IsAdmin ? null : await GetScopeAsync()))
            .Select(r => new LookupItem(r.Id, r.Name)).ToList();

    public async Task<List<LookupItem>> GetProvidersAsync(CancellationToken ct) =>
        (await uow.Providers.GetIdNamesAsync(IsAdmin ? null : ProviderIdsClaim()))
            .Select(p => new LookupItem(p.Id, p.Name, p.Short)).ToList();

    public UserPolicy GetPolicy()
    {
        var p = identityOptions.Value.Password;
        var n = userNamePolicy.Value;
        var rules = new List<string> { $"At least {p.RequiredLength} characters long" };
        if (p.RequireLowercase) rules.Add("At least one lower case letter");
        if (p.RequireUppercase) rules.Add("At least one upper case letter");
        if (p.RequireDigit) rules.Add("At least one number");
        if (p.RequireNonAlphanumeric) rules.Add("At least one special character");
        if (p.RequiredUniqueChars > 1) rules.Add($"At least {p.RequiredUniqueChars} different characters");
        return new(rules.ToArray(),
            $"{n.MinLength}-{n.MaxLength} characters, starting with {string.Join(" or ", n.AdAccountPrefixes.Select(x => x.ToUpperInvariant()))}, no @");
    }

    // ---------------------------------------------------------------- write

    public async Task<UserDetail> CreateAsync(UserEditRequest r, CancellationToken ct)
    {
        var errors = Validate(r, isNew: true);
        var roles = await ResolveRolesAsync(r.RoleIds, errors);
        if (errors.Count > 0) throw new ValidationFailedException(errors.ToDictionary(e => e.Key, e => e.Value.ToArray()));

        if (!RolesWithinScope(roles.Select(x => x.Name)))
            throw new ForbiddenAccessException("You can only manage users and roles within your role scope.");

        // Administrators have unrestricted access, so provider assignments mean nothing for them.
        var providerIds = HasAdminRole(roles) ? [] : r.ProviderIds.Distinct().ToList();
        if (!ProvidersWithinScope(providerIds))
            throw new ForbiddenAccessException("You can only manage users within your own provider scope.");

        var email = r.Email!.Trim();
        var userName = r.IsADAccount ? r.UserName!.Trim() : email;
        var isAd = r.IsADAccount;

        if (await uow.Users.UserNameExistsAsync(users.NormalizeName(userName)))
            throw isAd
                ? ValidationFailedException.For("userName", "User ID must be unique.")
                : ValidationFailedException.For("email", "An account with this email address already exists.");

        var u = new ApplicationUser
        {
            UserName = userName,
            Email = email,
            EmailConfirmed = true,
            FirstName = r.FirstName!.Trim(),
            LastName = r.LastName!.Trim(),
            PhoneNumber = Blank(r.PhoneNumber),
            Comment = r.Notes ?? "",
            IsActive = r.IsEnabled,
            IsADAccount = isAd,
            // AD accounts get MFA from Entra, so the flag only means something for local ones.
            TwoFactorEnabled = r.TwoFactorEnabled && !isAd,
            // An admin-issued password is temporary by definition.
            IsTemporaryPassword = !isAd
        };

        await using var tx = await uow.BeginTransactionAsync(ct);

        var created = isAd ? await users.CreateAsync(u) : await users.CreateAsync(u, r.Password!);
        if (!created.Succeeded) throw ToValidation(created, "password");

        if (!isAd) history.Record(u);
        uow.UserRoles.AddRange(roles.Select(x => new ApplicationUserRole { UserId = u.Id, RoleId = x.Id }));
        uow.ProviderUsers.AddRange(providerIds.Select(pid => new ProviderUser { UserId = u.Id, ProviderId = pid }));
        await uow.SaveChangesAsync();
        await tx.CommitAsync(ct);

        return (await GetAsync(u.UserKey, ct)).Detail!;
    }

    // Null = user not found.
    public async Task<UserDetail?> UpdateAsync(Guid userKey, UserEditRequest r, CancellationToken ct)
    {
        var u = await uow.Users.GetByKeyAsync(userKey);
        if (u is null) return null;

        var errors = Validate(r, isNew: false, existing: u);
        var desiredRoles = await ResolveRolesAsync(r.RoleIds, errors);

        // Account type, user ID and a local account email are fixed at creation; reject rather than
        // silently ignore, so a caller editing a frozen field hears about it.
        if (r.IsADAccount != u.IsADAccount) Add(errors, "isADAccount", "Account type cannot be changed after the account is created.");
        if (!u.IsADAccount && !string.Equals(r.Email?.Trim(), u.Email, StringComparison.OrdinalIgnoreCase))
            Add(errors, "email", "Email cannot be changed for a local account, it is the user ID.");
        if (errors.Count > 0) throw new ValidationFailedException(errors.ToDictionary(e => e.Key, e => e.Value.ToArray()));

        var currentRoles = await uow.UserRoles.GetRolesForUserAsync(u.Id);
        if (!RolesWithinScope(currentRoles.Select(x => x.Name).Concat(desiredRoles.Select(x => x.Name))))
            throw new ForbiddenAccessException("You can only manage users and roles within your role scope.");

        var desiredProviderIds = HasAdminRole(desiredRoles) ? [] : r.ProviderIds.Distinct().ToList();
        var currentProviderRows = await uow.ProviderUsers.GetForUserAsync(u.Id);
        if (!ProvidersWithinScope(currentProviderRows.Select(x => x.ProviderId).Concat(desiredProviderIds)))
            throw new ForbiddenAccessException("You can only manage users within your own provider scope.");

        // Optimistic concurrency: fail if the row changed since the form was loaded. Compared here
        // rather than via the tracked OriginalValue, which UserManager validation queries overwrite.
        // The window between this check and the save is covered by Identity ConcurrencyStamp.
        if (Has(r.RowVersion) && !u.Version.AsSpan().SequenceEqual(Convert.FromBase64String(r.RowVersion!)))
            throw new ConflictException("This user was changed by someone else. Reload the page and try again.");

        u.FirstName = r.FirstName!.Trim();
        u.LastName = r.LastName!.Trim();
        u.PhoneNumber = Blank(r.PhoneNumber);
        u.Comment = r.Notes ?? "";
        u.IsActive = r.IsEnabled;
        if (u.IsADAccount) u.Email = r.Email!.Trim();   // an AD email is contact detail only
        // Turning this off stops requiring a code but keeps any enrollment.
        u.TwoFactorEnabled = r.TwoFactorEnabled && !u.IsADAccount;

        await using var tx = await uow.BeginTransactionAsync(ct);

        var updated = await users.UpdateAsync(u);
        if (!updated.Succeeded) throw ToValidation(updated, "form");

        var desiredIds = desiredRoles.Select(x => x.Id).ToHashSet();
        var currentRoleIds = currentRoles.Select(x => x.Id).ToHashSet();
        uow.UserRoles.RemoveRange((await uow.UserRoles.GetForUserAsync(u.Id)).Where(x => !desiredIds.Contains(x.RoleId)));
        uow.UserRoles.AddRange(desiredIds.Except(currentRoleIds).Select(id => new ApplicationUserRole { UserId = u.Id, RoleId = id }));

        uow.ProviderUsers.RemoveRange(currentProviderRows.Where(x => !desiredProviderIds.Contains(x.ProviderId)));
        uow.ProviderUsers.AddRange(desiredProviderIds.Except(currentProviderRows.Select(x => x.ProviderId))
            .Select(pid => new ProviderUser { UserId = u.Id, ProviderId = pid }));

        await uow.SaveChangesAsync();
        await tx.CommitAsync(ct);

        return (await GetAsync(userKey, ct)).Detail;
    }

    // False = user not found.
    public async Task<bool> SetPasswordAsync(Guid userKey, SetPasswordRequest r, CancellationToken ct)
    {
        var u = await uow.Users.GetByKeyAsync(userKey);
        if (u is null) return false;

        if (u.IsADAccount)
            throw ValidationFailedException.For("password", "AD accounts do not use local passwords.");

        // Resetting a password is account takeover if unscoped, so it follows the same scope as edit.
        await EnsureTargetInScopeAsync(u);

        var errors = new Dictionary<string, List<string>>();
        foreach (var msg in PasswordErrors(r.Password)) Add(errors, "password", msg);
        if (r.Password != r.ConfirmPassword) Add(errors, "confirmPassword", "Passwords must match.");
        if (errors.Count > 0) throw new ValidationFailedException(errors.ToDictionary(e => e.Key, e => e.Value.ToArray()));

        if (await history.IsReusedAsync(u, r.Password!, ct))
            throw ValidationFailedException.For("password", $"You cannot reuse any of your last {PasswordHistory.Depth} passwords.");

        await using var tx = await uow.BeginTransactionAsync(ct);

        // Remove then add: a reset without needing a token provider. Legacy-only users have no hash to remove.
        if (await users.HasPasswordAsync(u)) await users.RemovePasswordAsync(u);
        u.IsTemporaryPassword = true;   // the user must change an admin-issued password
        u.LegacyPassword = "";
        var added = await users.AddPasswordAsync(u, r.Password!);
        if (!added.Succeeded) throw ToValidation(added, "password");
        var flags = await users.UpdateAsync(u);
        if (!flags.Succeeded) throw ToValidation(flags, "form");

        history.Record(u);
        await uow.SaveChangesAsync();
        await tx.CommitAsync(ct);
        return true;
    }

    // False = user not found.
    public async Task<bool> DeleteAsync(Guid userKey, CancellationToken ct)
    {
        var u = await uow.Users.GetByKeyAsync(userKey);
        if (u is null) return false;

        if (u.Id.ToString() == Principal.FindFirstValue(ClaimTypes.NameIdentifier))
            throw new ForbiddenAccessException("You cannot delete your own account.");
        await EnsureTargetInScopeAsync(u);

        // Documents and facility links have no screen here yet, so refuse rather than orphan or destroy them.
        if (await uow.Documents.AnyForUserAsync(u.Id) || await uow.FacilityUsers.AnyForUserAsync(u.Id))
            throw new ConflictException("This user has documents or facility assignments. Remove them (agreements: View User Agreement) before deleting the user.");

        await using var tx = await uow.BeginTransactionAsync(ct);

        // The account own housekeeping rows; their FKs are NO ACTION, so they go first.
        uow.UserRoles.RemoveRange(await uow.UserRoles.GetForUserAsync(u.Id));
        uow.ProviderUsers.RemoveRange(await uow.ProviderUsers.GetForUserAsync(u.Id));
        uow.PasswordChangeLogs.RemoveRange(await uow.PasswordChangeLogs.GetForUserAsync(u.Id));
        await uow.SaveChangesAsync();
        await uow.Logons.DeleteForUserAsync(u.Id);

        var deleted = await users.DeleteAsync(u);
        if (!deleted.Succeeded) throw ToValidation(deleted, "form");
        await tx.CommitAsync(ct);
        return true;
    }

    // Id of a user the caller may see and manage; null = not found. Throws Forbidden outside the caller scope.
    public async Task<int?> ResolveScopedUserIdAsync(Guid userKey)
    {
        var u = await uow.Users.GetByKeyNoTrackingAsync(userKey);
        if (u is null) return null;
        await EnsureTargetInScopeAsync(u);
        return u.Id;
    }

    // Same rule as edit: every role and provider of the target must be inside the caller scope.
    private async Task EnsureTargetInScopeAsync(ApplicationUser u)
    {
        var roles = await uow.UserRoles.GetRolesForUserAsync(u.Id);
        if (!RolesWithinScope(roles.Select(r => r.Name)))
            throw new ForbiddenAccessException("You can only manage users and roles within your role scope.");

        if (!ProvidersWithinScope(await uow.ProviderUsers.GetProviderIdsForUserAsync(u.Id)))
            throw new ForbiddenAccessException("You can only manage users within your own provider scope.");
    }

    // ---------------------------------------------------------------- validation

    private Dictionary<string, List<string>> Validate(UserEditRequest r, bool isNew, ApplicationUser? existing = null)
    {
        var e = new Dictionary<string, List<string>>();
        var isAd = existing?.IsADAccount ?? r.IsADAccount;

        if (isNew && isAd)
        {
            var n = userNamePolicy.Value;
            var v = r.UserName?.Trim();
            if (string.IsNullOrEmpty(v)) Add(e, "userName", "User ID is required.");
            else if (v.Length < n.MinLength) Add(e, "userName", $"User ID must be at least {n.MinLength} characters in length.");
            else if (v.Length > n.MaxLength) Add(e, "userName", $"User ID cannot exceed {n.MaxLength} characters in length.");
            else if (v.Contains('@')) Add(e, "userName", "User ID cannot contain '@'.");
            else if (!n.AdAccountPrefixes.Any(p => v.StartsWith(p, StringComparison.OrdinalIgnoreCase)))
                Add(e, "userName", $"User ID must start with {string.Join(" or ", n.AdAccountPrefixes.Select(x => x.ToUpperInvariant()))}.");
        }

        RequireText(e, "firstName", "First name", r.FirstName, MaxNameLength);
        RequireText(e, "lastName", "Last name", r.LastName, MaxNameLength);

        var email = r.Email?.Trim();
        if (string.IsNullOrEmpty(email)) Add(e, "email", "Email is required.");
        else if (!new EmailAddressAttribute().IsValid(email)) Add(e, "email", "Enter a valid email address.");
        else if (email.Length > (isAd ? MaxEmailLength : MaxLocalUserNameLength))
            Add(e, "email", $"Email cannot exceed {(isAd ? MaxEmailLength : MaxLocalUserNameLength)} characters in length.");

        if (PhoneError(r.PhoneNumber) is string phone) Add(e, "phoneNumber", phone);

        if (isAd)
        {
            if (Has(r.Password)) Add(e, "password", "AD accounts should not have passwords set here.");
        }
        else if (isNew)
        {
            foreach (var msg in PasswordErrors(r.Password)) Add(e, "password", msg);
        }
        if (isNew && Has(r.Password) && r.Password != r.ConfirmPassword) Add(e, "confirmPassword", "Passwords must match.");

        if (r.RoleIds.Count == 0) Add(e, "roleIds", "At least one role must be selected.");
        return e;
    }

    private IEnumerable<string> PasswordErrors(string? pw)
    {
        var p = identityOptions.Value.Password;
        if (string.IsNullOrWhiteSpace(pw)) { yield return "Password is required."; yield break; }
        if (pw.Length > 128) yield return "Password cannot exceed 128 characters.";
        if (p.RequireLowercase && !pw.Any(char.IsLower)) yield return "At least one lower case letter";
        if (p.RequireUppercase && !pw.Any(char.IsUpper)) yield return "At least one upper case letter";
        if (p.RequireDigit && !pw.Any(char.IsDigit)) yield return "At least one number";
        if (p.RequireNonAlphanumeric && pw.All(char.IsLetterOrDigit)) yield return "At least one special character";
        if (pw.Length < p.RequiredLength) yield return $"At least {p.RequiredLength} characters long";
        if (p.RequiredUniqueChars > 1 && pw.Distinct().Count() < p.RequiredUniqueChars) yield return $"At least {p.RequiredUniqueChars} different characters";
    }

    // A leading + is the only non-digit that carries meaning; 10-15 digits, the rest is punctuation.
    private static string? PhoneError(string? phone)
    {
        var v = phone?.Trim();
        if (string.IsNullOrEmpty(v)) return null;
        if (v.Length > MaxPhoneLength) return $"Phone number cannot exceed {MaxPhoneLength} characters in length.";
        var body = v.StartsWith('+') ? v[1..] : v;
        var digits = body.Count(char.IsDigit);
        return body.All(c => char.IsDigit(c) || " ().-".Contains(c)) && digits is >= 10 and <= 15
            ? null : "Enter a valid phone number, for example (555) 123-4567.";
    }

    private static void RequireText(Dictionary<string, List<string>> e, string field, string label, string? v, int max)
    {
        if (string.IsNullOrWhiteSpace(v)) Add(e, field, $"{label} is required.");
        else if (v.Trim().Length > max) Add(e, field, $"{label} cannot exceed {max} characters in length.");
    }

    // Identity errors: password problems land on the password field, the rest (concurrency, ...) are form-level.
    private static Exception ToValidation(IdentityResult result, string defaultField)
    {
        if (result.Errors.Any(x => x.Code == "ConcurrencyFailure"))
            return new ConflictException("This user was changed by someone else. Reload the page and try again.");

        var errors = new Dictionary<string, List<string>>();
        foreach (var err in result.Errors)
            Add(errors, err.Code.StartsWith("Password") ? "password" : defaultField, err.Description);
        return new ValidationFailedException(errors.ToDictionary(x => x.Key, x => x.Value.ToArray()));
    }

    private static void Add(Dictionary<string, List<string>> e, string field, string message)
    {
        if (!e.TryGetValue(field, out var list)) e[field] = list = [];
        list.Add(message);
    }

    // ---------------------------------------------------------------- scope

    private async Task<List<(int Id, string Name)>> ResolveRolesAsync(List<int> ids, Dictionary<string, List<string>> errors)
    {
        var distinct = ids.Distinct().ToList();
        var found = (await uow.Roles.GetIdNamesAsync(distinct)).Select(r => (r.Id, r.Name)).ToList();
        if (found.Count != distinct.Count) Add(errors, "roleIds", "One or more selected roles do not exist.");
        return found;
    }

    // Admins: unrestricted (empty set is unused). Others: role ids whose slug they hold a users.role.<slug> claim for.
    private async Task<HashSet<int>> GetScopeAsync()
    {
        if (IsAdmin) return [];
        var slugs = Slugs();
        if (slugs.Count == 0) return [];
        return (await uow.Roles.GetIdNamesAsync()).Where(r => slugs.Contains(Slug(r.Name))).Select(r => r.Id).ToHashSet();
    }

    private static bool HasAdminRole(IEnumerable<(int Id, string Name)> roles) =>
        roles.Any(r => string.Equals(r.Name, AppRoles.Admin, StringComparison.OrdinalIgnoreCase));

    // Every role must be in scope, and a caller with no users.role.* claims has no scope at all.
    private bool RolesWithinScope(IEnumerable<string> roleNames)
    {
        if (IsAdmin) return true;
        var slugs = Slugs();
        return slugs.Count > 0 && roleNames.All(n => slugs.Contains(Slug(n)));
    }

    private bool ProvidersWithinScope(IEnumerable<int> providerIds)
    {
        if (IsAdmin) return true;
        var allowed = ProviderIdsClaim();
        return allowed.Length > 0 && providerIds.All(allowed.Contains);
    }

    private HashSet<string> Slugs() => Principal.FindAll(AppClaimTypes.Permission)
        .Where(c => c.Value.StartsWith(RolePrefix, StringComparison.OrdinalIgnoreCase))
        .Select(c => c.Value[RolePrefix.Length..]).Where(v => v != "").ToHashSet(StringComparer.OrdinalIgnoreCase);

    private int[] ProviderIdsClaim() =>
        (Principal.FindFirstValue(AppClaimTypes.ProviderIds) ?? "")
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(v => int.TryParse(v, out var id) ? id : 0).Where(id => id > 0).ToArray();

    private static string Slug(string s) => UserRolePermissionConstants.Slug(s);

    private static bool Has(string? s) => !string.IsNullOrWhiteSpace(s);
    private static string? Blank(string? s) => Has(s) ? s!.Trim() : null;
}
