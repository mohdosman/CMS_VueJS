using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Text.RegularExpressions;
using CMS.Data.Context;
using CMS.Data.Models;
using CMS.Features.Users.ViewModels;
using CMS.Shared.Common;
using CMS.Shared.Constants;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace CMS.Features.Users.Services;

// Port of the Blazor CMS UserService (search, read, create, update). Non-admins are scoped two ways:
//  - roles: only users/roles matching a "users.role.<slug>" permission claim
//  - providers: only users sharing a provider with the provider_ids claim of the caller
// Delete, set-password, MFA reset and agreement documents are not ported yet.
public sealed partial class UserService(
    AppDbContext db,
    UserManager<ApplicationUser> users,
    IHttpContextAccessor http,
    IOptions<IdentityOptions> identityOptions,
    IOptions<UserNamePolicyOptions> userNamePolicy)
{
    private const string RolePrefix = "users.role.";
    private const int MaxNameLength = 256, MaxEmailLength = 100, MaxLocalUserNameLength = 50, MaxPhoneLength = 32;

    private ClaimsPrincipal Principal => http.HttpContext!.User;
    private bool IsAdmin => Principal.IsInRole(AppRoles.Admin);

    // ---------------------------------------------------------------- read

    public async Task<PagedResult<UserListItem>> SearchAsync(UserSearchRequest req, CancellationToken ct)
    {
        var scope = await GetScopeAsync(ct);
        var q = db.Users.AsNoTracking();

        if (!IsAdmin)
        {
            var providerIds = ProviderIdsClaim();
            if (providerIds.Length == 0 || scope.Count == 0) return new();

            q = q.Where(u => db.UserRoles.Any(ur => ur.UserId == u.Id && scope.Contains(ur.RoleId)));
            q = q.Where(u => db.ProviderUsers.Any(pu => pu.UserId == u.Id && providerIds.Contains(pu.ProviderId)));
        }

        if (Has(req.UserName)) { var v = $"%{req.UserName!.Trim()}%"; q = q.Where(u => EF.Functions.Like(u.UserName!, v)); }
        if (Has(req.FirstName)) { var v = $"%{req.FirstName!.Trim()}%"; q = q.Where(u => EF.Functions.Like(u.FirstName, v)); }
        if (Has(req.LastName)) { var v = $"%{req.LastName!.Trim()}%"; q = q.Where(u => EF.Functions.Like(u.LastName, v)); }
        if (Has(req.Email)) { var v = $"%{req.Email!.Trim()}%"; q = q.Where(u => EF.Functions.Like(u.Email!, v)); }

        if (ToBool(req.IsEnabled) is bool enabled) q = q.Where(u => u.IsActive == enabled);
        if (ToBool(req.IsADAccount) is bool ad) q = q.Where(u => u.IsADAccount == ad);

        var now = DateTimeOffset.UtcNow;
        if (ToBool(req.IsLockedOut) is bool locked)
            q = locked
                ? q.Where(u => u.LockoutEnabled && u.LockoutEnd >= now)
                : q.Where(u => !u.LockoutEnabled || u.LockoutEnd == null || u.LockoutEnd < now);

        if (req.RoleIds.Count > 0)
        {
            // A non-admin can only filter by roles inside their own scope.
            var roleIds = IsAdmin ? req.RoleIds : req.RoleIds.Where(scope.Contains).ToList();
            if (roleIds.Count == 0) return new();
            q = q.Where(u => db.UserRoles.Any(ur => ur.UserId == u.Id && roleIds.Contains(ur.RoleId)));
        }
        if (req.ProviderIds.Count > 0)
        {
            var providerIds = req.ProviderIds;
            q = q.Where(u => db.ProviderUsers.Any(pu => pu.UserId == u.Id && providerIds.Contains(pu.ProviderId)));
        }

        q = (req.SortBy ?? "").ToLowerInvariant() switch
        {
            "firstname" => Order(q, u => u.FirstName, req.SortDesc),
            "lastname" => Order(q, u => u.LastName, req.SortDesc),
            "email" => Order(q, u => u.Email!, req.SortDesc),
            "isadaccount" => Order(q, u => u.IsADAccount, req.SortDesc),
            "isenabled" => Order(q, u => u.IsActive, req.SortDesc),
            "islockedout" => Order(q, u => u.LockoutEnabled && u.LockoutEnd >= now, req.SortDesc),
            _ => Order(q, u => u.UserName!, req.SortDesc)
        };

        var size = Math.Clamp(req.PageSize, 1, 200);
        var page = Math.Max(1, req.PageIndex);
        var total = await q.CountAsync(ct);
        var items = await q.Skip((page - 1) * size).Take(size).Select(u => new UserListItem
        {
            UserKey = u.UserKey,
            UserName = u.UserName!,
            FirstName = u.FirstName,
            LastName = u.LastName,
            Email = u.Email ?? "",
            IsEnabled = u.IsActive,
            IsADAccount = u.IsADAccount,
            IsLockedOut = u.LockoutEnabled && u.LockoutEnd >= now
        }).ToListAsync(ct);

        return new() { Items = items, TotalCount = total };
    }

    // Detail null + Allowed true = not found; Allowed false = a target role is outside the caller scope.
    public async Task<(UserDetail? Detail, bool Allowed)> GetAsync(Guid userKey, CancellationToken ct)
    {
        var u = await db.Users.AsNoTracking().FirstOrDefaultAsync(x => x.UserKey == userKey, ct);
        if (u is null) return (null, true);

        var roles = await (from ur in db.UserRoles join r in db.Roles on ur.RoleId equals r.Id
                           where ur.UserId == u.Id orderby r.Name select new { r.Id, Name = r.Name! }).ToListAsync(ct);
        if (!IsAdmin && !RolesWithinScope(roles.Select(r => r.Name))) return (null, false);

        var providers = await (from pu in db.ProviderUsers join p in db.Providers on pu.ProviderId equals p.ProviderId
                               where pu.UserId == u.Id orderby p.Name select new { p.ProviderId, p.Name }).ToListAsync(ct);
        var lastLogon = await db.Logons.AsNoTracking().Where(l => l.UserId == u.Id)
            .Select(l => (DateTime?)l.LogOnDateTime).MaxAsync(ct);
        DateTime? own = u.LastLoginDate > new DateTime(1900, 1, 1) ? u.LastLoginDate : null;
        var lastLogin = lastLogon > own ? lastLogon : own;

        return (new UserDetail
        {
            UserKey = u.UserKey,
            RowVersion = Convert.ToBase64String(u.Version),
            UserName = u.UserName!,
            FirstName = u.FirstName,
            LastName = u.LastName,
            Email = u.Email ?? "",
            PhoneNumber = u.PhoneNumber,
            Notes = u.Comment ?? "",
            IsEnabled = u.IsActive,
            IsADAccount = u.IsADAccount,
            IsLockedOut = u.LockoutEnabled && u.LockoutEnd >= DateTimeOffset.UtcNow,
            TwoFactorEnabled = u.TwoFactorEnabled,
            LastLoginAt = lastLogin,
            LastPasswordChangedDate = u.LastPasswordChangedDate,
            CreatedOn = u.CreatedOn,
            UpdatedOn = u.UpdatedOn,
            Roles = roles.Select(r => r.Name).ToList(),
            RoleIds = roles.Select(r => r.Id).ToList(),
            Providers = providers.Select(p => p.Name).ToList(),
            ProviderIds = providers.Select(p => p.ProviderId).ToList()
        }, true);
    }

    public async Task<List<LookupItem>> GetRolesAsync(CancellationToken ct)
    {
        var scope = await GetScopeAsync(ct);
        var q = db.Roles.AsNoTracking();
        if (!IsAdmin) q = q.Where(r => scope.Contains(r.Id));
        return await q.OrderBy(r => r.Name).Select(r => new LookupItem(r.Id, r.Name!)).ToListAsync(ct);
    }

    public async Task<List<LookupItem>> GetProvidersAsync(CancellationToken ct)
    {
        var q = db.Providers.AsNoTracking();
        if (!IsAdmin)
        {
            var ids = ProviderIdsClaim();
            q = q.Where(p => ids.Contains(p.ProviderId));
        }
        return await q.OrderBy(p => p.Name).Select(p => new LookupItem(p.ProviderId, p.Name)).ToListAsync(ct);
    }

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
        var roles = await ResolveRolesAsync(r.RoleIds, errors, ct);
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

        var normalized = users.NormalizeName(userName);
        if (await db.Users.AnyAsync(x => x.NormalizedUserName == normalized, ct))
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

        await using var tx = await db.Database.BeginTransactionAsync(ct);

        var created = isAd ? await users.CreateAsync(u) : await users.CreateAsync(u, r.Password!);
        if (!created.Succeeded) throw ToValidation(created, "password");

        if (!isAd) RecordPassword(u);
        db.UserRoles.AddRange(roles.Select(x => new ApplicationUserRole { UserId = u.Id, RoleId = x.Id }));
        db.ProviderUsers.AddRange(providerIds.Select(pid => new ProviderUser { UserId = u.Id, ProviderId = pid }));
        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);

        return (await GetAsync(u.UserKey, ct)).Detail!;
    }

    // Null = user not found.
    public async Task<UserDetail?> UpdateAsync(Guid userKey, UserEditRequest r, CancellationToken ct)
    {
        var u = await db.Users.FirstOrDefaultAsync(x => x.UserKey == userKey, ct);
        if (u is null) return null;

        var errors = Validate(r, isNew: false, existing: u);
        var desiredRoles = await ResolveRolesAsync(r.RoleIds, errors, ct);

        // Account type, user ID and a local account email are fixed at creation; reject rather than
        // silently ignore, so a caller editing a frozen field hears about it.
        if (r.IsADAccount != u.IsADAccount) Add(errors, "isADAccount", "Account type cannot be changed after the account is created.");
        if (!u.IsADAccount && !string.Equals(r.Email?.Trim(), u.Email, StringComparison.OrdinalIgnoreCase))
            Add(errors, "email", "Email cannot be changed for a local account, it is the user ID.");
        if (errors.Count > 0) throw new ValidationFailedException(errors.ToDictionary(e => e.Key, e => e.Value.ToArray()));

        var currentRoleIds = await db.UserRoles.Where(x => x.UserId == u.Id).Select(x => x.RoleId).ToListAsync(ct);
        var currentRoleNames = await db.Roles.Where(x => currentRoleIds.Contains(x.Id)).Select(x => x.Name!).ToListAsync(ct);
        if (!RolesWithinScope(currentRoleNames.Concat(desiredRoles.Select(x => x.Name))))
            throw new ForbiddenAccessException("You can only manage users and roles within your role scope.");

        var desiredProviderIds = HasAdminRole(desiredRoles) ? [] : r.ProviderIds.Distinct().ToList();
        var currentProviderRows = await db.ProviderUsers.Where(x => x.UserId == u.Id).ToListAsync(ct);
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

        await using var tx = await db.Database.BeginTransactionAsync(ct);

        var updated = await users.UpdateAsync(u);
        if (!updated.Succeeded) throw ToValidation(updated, "form");

        var desiredIds = desiredRoles.Select(x => x.Id).ToHashSet();
        db.UserRoles.RemoveRange(await db.UserRoles.Where(x => x.UserId == u.Id && !desiredIds.Contains(x.RoleId)).ToListAsync(ct));
        db.UserRoles.AddRange(desiredIds.Except(currentRoleIds).Select(id => new ApplicationUserRole { UserId = u.Id, RoleId = id }));

        db.ProviderUsers.RemoveRange(currentProviderRows.Where(x => !desiredProviderIds.Contains(x.ProviderId)));
        db.ProviderUsers.AddRange(desiredProviderIds.Except(currentProviderRows.Select(x => x.ProviderId))
            .Select(pid => new ProviderUser { UserId = u.Id, ProviderId = pid }));

        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);

        return (await GetAsync(userKey, ct)).Detail;
    }

    // False = user not found.
    public async Task<bool> SetPasswordAsync(Guid userKey, SetPasswordRequest r, CancellationToken ct)
    {
        var u = await db.Users.FirstOrDefaultAsync(x => x.UserKey == userKey, ct);
        if (u is null) return false;

        if (u.IsADAccount)
            throw ValidationFailedException.For("password", "AD accounts do not use local passwords.");

        // Resetting a password is account takeover if unscoped, so it follows the same scope as edit.
        await EnsureTargetInScopeAsync(u, ct);

        var errors = new Dictionary<string, List<string>>();
        foreach (var msg in PasswordErrors(r.Password)) Add(errors, "password", msg);
        if (r.Password != r.ConfirmPassword) Add(errors, "confirmPassword", "Passwords must match.");
        if (errors.Count > 0) throw new ValidationFailedException(errors.ToDictionary(e => e.Key, e => e.Value.ToArray()));

        if (await IsReusedAsync(u, r.Password!, ct))
            throw ValidationFailedException.For("password", $"You cannot reuse any of your last {PasswordHistoryDepth} passwords.");

        await using var tx = await db.Database.BeginTransactionAsync(ct);

        // Remove then add: a reset without needing a token provider. Legacy-only users have no hash to remove.
        if (await users.HasPasswordAsync(u)) await users.RemovePasswordAsync(u);
        u.IsTemporaryPassword = true;   // the user must change an admin-issued password
        u.LegacyPassword = "";
        var added = await users.AddPasswordAsync(u, r.Password!);
        if (!added.Succeeded) throw ToValidation(added, "password");
        var flags = await users.UpdateAsync(u);
        if (!flags.Succeeded) throw ToValidation(flags, "form");

        RecordPassword(u);
        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);
        return true;
    }

    // False = user not found.
    public async Task<bool> DeleteAsync(Guid userKey, CancellationToken ct)
    {
        var u = await db.Users.FirstOrDefaultAsync(x => x.UserKey == userKey, ct);
        if (u is null) return false;

        if (u.Id.ToString() == Principal.FindFirstValue(ClaimTypes.NameIdentifier))
            throw new ForbiddenAccessException("You cannot delete your own account.");
        await EnsureTargetInScopeAsync(u, ct);

        // Documents and facility links have no screen here yet, so refuse rather than orphan or destroy them.
        var documents = await db.Database.SqlQuery<int>($"SELECT COUNT(*) AS Value FROM dbo.CMS_Document WHERE UserId = {u.Id}").SingleAsync(ct);
        var facilities = await db.Database.SqlQuery<int>($"SELECT COUNT(*) AS Value FROM dbo.CMS_FacilityUser WHERE UserId = {u.Id}").SingleAsync(ct);
        if (documents > 0 || facilities > 0)
            throw new ConflictException("This user has documents or facility assignments. Remove those before deleting the user.");

        await using var tx = await db.Database.BeginTransactionAsync(ct);

        // The account own housekeeping rows; their FKs are NO ACTION, so they go first.
        db.UserRoles.RemoveRange(await db.UserRoles.Where(x => x.UserId == u.Id).ToListAsync(ct));
        db.ProviderUsers.RemoveRange(await db.ProviderUsers.Where(x => x.UserId == u.Id).ToListAsync(ct));
        db.PasswordChangeLogs.RemoveRange(await db.PasswordChangeLogs.Where(x => x.UserId == u.Id).ToListAsync(ct));
        await db.SaveChangesAsync(ct);
        await db.Database.ExecuteSqlInterpolatedAsync($"DELETE FROM dbo.RBS_Logon WHERE UserId = {u.Id}", ct);

        var deleted = await users.DeleteAsync(u);
        if (!deleted.Succeeded) throw ToValidation(deleted, "form");
        await tx.CommitAsync(ct);
        return true;
    }

    // Same rule as edit: every role and provider of the target must be inside the caller scope.
    private async Task EnsureTargetInScopeAsync(ApplicationUser u, CancellationToken ct)
    {
        var roleNames = await (from ur in db.UserRoles join r in db.Roles on ur.RoleId equals r.Id
                               where ur.UserId == u.Id select r.Name!).ToListAsync(ct);
        if (!RolesWithinScope(roleNames))
            throw new ForbiddenAccessException("You can only manage users and roles within your role scope.");

        var providerIds = await db.ProviderUsers.Where(x => x.UserId == u.Id).Select(x => x.ProviderId).ToListAsync(ct);
        if (!ProvidersWithinScope(providerIds))
            throw new ForbiddenAccessException("You can only manage users within your own provider scope.");
    }

    // ---------------------------------------------------------------- password history

    private const int PasswordHistoryDepth = 12;

    private async Task<bool> IsReusedAsync(ApplicationUser u, string password, CancellationToken ct)
    {
        var hashes = await db.PasswordChangeLogs.AsNoTracking()
            .Where(x => x.UserId == u.Id && x.PasswordHash != null)
            .OrderByDescending(x => x.CreatedOn).Take(PasswordHistoryDepth)
            .Select(x => x.PasswordHash!).ToListAsync(ct);
        if (!string.IsNullOrEmpty(u.PasswordHash)) hashes.Add(u.PasswordHash);   // the current one counts too

        return hashes.Any(h => users.PasswordHasher.VerifyHashedPassword(u, h, password) != PasswordVerificationResult.Failed);
    }

    // Queued on the context; the caller saves it in the same transaction as the password change.
    private void RecordPassword(ApplicationUser u) =>
        db.PasswordChangeLogs.Add(new PasswordChangeLog { UserId = u.Id, PasswordHash = u.PasswordHash });

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

    private async Task<List<(int Id, string Name)>> ResolveRolesAsync(List<int> ids, Dictionary<string, List<string>> errors, CancellationToken ct)
    {
        var distinct = ids.Distinct().ToList();
        var found = (await db.Roles.AsNoTracking().Where(r => distinct.Contains(r.Id)).Select(r => new { r.Id, r.Name }).ToListAsync(ct))
            .Select(r => (r.Id, Name: r.Name!)).ToList();
        if (found.Count != distinct.Count) Add(errors, "roleIds", "One or more selected roles do not exist.");
        return found;
    }

    private static bool HasAdminRole(IEnumerable<(int Id, string Name)> roles) =>
        roles.Any(r => string.Equals(r.Name, AppRoles.Admin, StringComparison.OrdinalIgnoreCase));

    // Admins: unrestricted (empty set is unused). Others: role ids whose slug they hold a users.role.<slug> claim for.
    private async Task<HashSet<int>> GetScopeAsync(CancellationToken ct)
    {
        if (IsAdmin) return [];
        var slugs = Slugs();
        if (slugs.Count == 0) return [];
        var all = await db.Roles.AsNoTracking().Select(r => new { r.Id, r.Name }).ToListAsync(ct);
        return all.Where(r => r.Name is not null && slugs.Contains(Slug(r.Name))).Select(r => r.Id).ToHashSet();
    }

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

    // Same slug rule as the Blazor CMS RemoveSpecialCharacters().
    private static string Slug(string s) => NonSlug().Replace(s, "").ToLowerInvariant();
    [GeneratedRegex("[^a-zA-Z0-9_.]+")] private static partial Regex NonSlug();

    private static bool Has(string? s) => !string.IsNullOrWhiteSpace(s);
    private static string? Blank(string? s) => Has(s) ? s!.Trim() : null;
    private static bool? ToBool(YesNoFilter f) => f switch { YesNoFilter.Yes => true, YesNoFilter.No => false, _ => null };
    private static IQueryable<ApplicationUser> Order<T>(IQueryable<ApplicationUser> q, Expression<Func<ApplicationUser, T>> key, bool desc) =>
        desc ? q.OrderByDescending(key) : q.OrderBy(key);
}
