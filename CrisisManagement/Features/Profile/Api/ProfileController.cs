using CrisisManagement.Data.Models.Identity;
using CrisisManagement.Infrastructure.Identity;
using CrisisManagement.Shared.Api;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace CrisisManagement.Features.Profile.Api;

public sealed record UserProfile(string UserName, string Email, string? FirstName, string? LastName, bool IsAdAccount, bool IsLockedOut, bool MfaRequired, bool MfaEnrolled);

// My Profile: read-only. Names are administered from the Users screen; authenticator setup is the server-rendered
// Account/SetupMfa page, so this only reports its status.
[Authorize]
public sealed class ProfileController(UserManager<ApplicationUser> users, MfaService mfa, ILogger<ProfileController> logger) : BaseApiController(logger)
{
    [HttpGet]
    public async Task<IActionResult> Get()
    {
        try
        {
            var u = await users.GetUserAsync(User);
            if (u is null) return NotFound();
            return Ok(new UserProfile(u.UserName ?? "", u.Email ?? "", u.FirstName, u.LastName, u.IsADAccount, u.IsLockedOut, mfa.IsRequired(u), await mfa.IsEnrolledAsync(u)));
        }
        catch (Exception ex) { return Failure(ex, "Loading the profile"); }
    }
}
