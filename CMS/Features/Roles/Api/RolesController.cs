using CMS.Features.Roles.Services;
using CMS.Features.Roles.ViewModels;
using CMS.Features.Users.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CMS.Features.Roles.Api;

// Policy "roles.view" is also satisfied by "roles.edit" (see PermissionHandler).
[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "roles.view")]
public sealed class RolesController(RoleService roles) : ControllerBase
{
    [HttpPost("search")]
    public async Task<ActionResult<PagedResult<RoleListItem>>> Search([FromBody] RoleSearchRequest req, CancellationToken ct) =>
        await roles.SearchAsync(req, ct);

    [HttpGet("{id:int}")]
    public async Task<ActionResult<RoleDetail>> Get(int id, CancellationToken ct) =>
        await roles.GetAsync(id, ct) is { } detail ? detail : NotFound();

    [HttpGet("permissions")]
    public async Task<List<PermissionGroupItem>> Permissions(CancellationToken ct) => await roles.GetPermissionGroupsAsync(ct);

    [HttpPost]
    [Authorize(Policy = "roles.edit")]
    public async Task<ActionResult<RoleDetail>> Create([FromBody] RoleEditRequest req, CancellationToken ct)
    {
        var created = await roles.CreateAsync(req, ct);
        return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
    }

    [HttpPut("{id:int}")]
    [Authorize(Policy = "roles.edit")]
    public async Task<ActionResult<RoleDetail>> Update(int id, [FromBody] RoleEditRequest req, CancellationToken ct) =>
        await roles.UpdateAsync(id, req, ct) is { } updated ? updated : NotFound();

    [HttpDelete("{id:int}")]
    [Authorize(Policy = "roles.edit")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct) =>
        await roles.DeleteAsync(id, ct) ? NoContent() : NotFound();
}
