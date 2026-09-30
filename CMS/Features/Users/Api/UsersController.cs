using CMS.Features.Users.Services;
using CMS.Features.Users.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CMS.Features.Users.Api;

// Policy "users.view" is also satisfied by "users.edit" (see PermissionHandler).
// Service exceptions become 400/403/409 through DomainExceptionFilter.
[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "users.view")]
public sealed class UsersController(UserService users) : ControllerBase
{
    [HttpPost("search")]
    public async Task<ActionResult<PagedResult<UserListItem>>> Search([FromBody] UserSearchRequest req, CancellationToken ct) =>
        await users.SearchAsync(req, ct);

    [HttpGet("{key:guid}")]
    public async Task<ActionResult<UserDetail>> Get(Guid key, CancellationToken ct)
    {
        var (detail, allowed) = await users.GetAsync(key, ct);
        if (!allowed) return Forbid();
        return detail is null ? NotFound() : detail;
    }

    [HttpGet("roles")]
    public async Task<List<LookupItem>> Roles(CancellationToken ct) => await users.GetRolesAsync(ct);

    [HttpGet("providers")]
    public async Task<List<LookupItem>> Providers(CancellationToken ct) => await users.GetProvidersAsync(ct);

    [HttpGet("policy")]
    public UserPolicy Policy() => users.GetPolicy();

    [HttpPost]
    [Authorize(Policy = "users.edit")]
    public async Task<ActionResult<UserDetail>> Create([FromBody] UserEditRequest req, CancellationToken ct)
    {
        var created = await users.CreateAsync(req, ct);
        return CreatedAtAction(nameof(Get), new { key = created.UserKey }, created);
    }

    [HttpPut("{key:guid}")]
    [Authorize(Policy = "users.edit")]
    public async Task<ActionResult<UserDetail>> Update(Guid key, [FromBody] UserEditRequest req, CancellationToken ct)
    {
        var updated = await users.UpdateAsync(key, req, ct);
        return updated is null ? NotFound() : updated;
    }

    [HttpPost("{key:guid}/password")]
    [Authorize(Policy = "users.edit")]
    public async Task<IActionResult> SetPassword(Guid key, [FromBody] SetPasswordRequest req, CancellationToken ct) =>
        await users.SetPasswordAsync(key, req, ct) ? NoContent() : NotFound();

    [HttpDelete("{key:guid}")]
    [Authorize(Policy = "users.edit")]
    public async Task<IActionResult> Delete(Guid key, CancellationToken ct) =>
        await users.DeleteAsync(key, ct) ? NoContent() : NotFound();
}
