using CMS.Data.Context;
using CMS.Data.Models.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CMS.Infrastructure.Identity;

// Last-12-passwords rule from the Blazor CMS PasswordHistoryService, shared by the Users screen
// (admin set password) and the Change Password page.
public sealed class PasswordHistory(AppDbContext db, UserManager<ApplicationUser> users)
{
    public const int Depth = 12;

    public async Task<bool> IsReusedAsync(ApplicationUser u, string password, CancellationToken ct)
    {
        var hashes = await db.PasswordChangeLogs.AsNoTracking()
            .Where(x => x.UserId == u.Id && x.PasswordHash != null)
            .OrderByDescending(x => x.CreatedOn).Take(Depth)
            .Select(x => x.PasswordHash!).ToListAsync(ct);
        if (!string.IsNullOrEmpty(u.PasswordHash)) hashes.Add(u.PasswordHash);   // the current one counts too

        return hashes.Any(h => users.PasswordHasher.VerifyHashedPassword(u, h, password) != PasswordVerificationResult.Failed);
    }

    // Queued on the context; the caller saves it in the same transaction as the password change.
    public void Record(ApplicationUser u) =>
        db.PasswordChangeLogs.Add(new PasswordChangeLog { UserId = u.Id, PasswordHash = u.PasswordHash });
}
