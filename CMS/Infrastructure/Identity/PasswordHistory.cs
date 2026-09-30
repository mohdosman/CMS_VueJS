using CMS.Data;
using CMS.Data.Models.Identity;
using Microsoft.AspNetCore.Identity;

namespace CMS.Infrastructure.Identity;

// Last-12-passwords rule from the Blazor CMS PasswordHistoryService, shared by the Users screen
// (admin set password) and the Change Password page.
public sealed class PasswordHistory(IUnitOfWork uow, UserManager<ApplicationUser> users)
{
    public const int Depth = 12;

    public async Task<bool> IsReusedAsync(ApplicationUser u, string password, CancellationToken ct)
    {
        var hashes = await uow.PasswordChangeLogs.GetRecentHashesAsync(u.Id, Depth);
        if (!string.IsNullOrEmpty(u.PasswordHash)) hashes.Add(u.PasswordHash);   // the current one counts too

        return hashes.Any(h => users.PasswordHasher.VerifyHashedPassword(u, h, password) != PasswordVerificationResult.Failed);
    }

    // Queued on the context; the caller saves it in the same transaction as the password change.
    public void Record(ApplicationUser u) =>
        uow.PasswordChangeLogs.Add(new PasswordChangeLog { UserId = u.Id, PasswordHash = u.PasswordHash });
}
