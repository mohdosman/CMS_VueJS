using CrisisManagement.Features.Roles.Services;
using CrisisManagement.Features.Roles.ViewModels;
using CrisisManagement.Shared.Api;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CrisisManagement.Features.Roles.Api;

// Policy "roles.view" is also satisfied by "roles.edit" (see PermissionHandler).
[Authorize(Policy = "roles.view")]
public sealed class RolesController(RoleService roles, ILogger<RolesController> logger) : BaseApiController(logger)
{
    private readonly ILogger<RolesController> _logger = logger;

    [HttpPost("search")]
    public async Task<IActionResult> Search([FromBody] RoleSearchRequest request, CancellationToken ct)
    {
        try
        {
            return Ok(await roles.SearchAsync(request, ct));
        }
        catch (Exception ex)
        {
            return Failure(ex, "Searching roles");
        }
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Get(int id, CancellationToken ct)
    {
        try
        {
            var detail = await roles.GetAsync(id, ct);
            return detail is null ? NotFound() : Ok(detail);
        }
        catch (Exception ex)
        {
            return Failure(ex, $"Loading role {id}");
        }
    }

    // The permission catalog, grouped and ordered like the menu, for the role screen.
    [HttpGet("permissions")]
    public async Task<IActionResult> Permissions(CancellationToken ct)
    {
        try
        {
            return Ok(await roles.GetPermissionGroupsAsync(ct));
        }
        catch (Exception ex)
        {
            return Failure(ex, "Loading the permission catalog");
        }
    }

    [HttpPost]
    [Authorize(Policy = "roles.edit")]
    public async Task<IActionResult> Create([FromBody] RoleEditRequest request, CancellationToken ct)
    {
        try
        {
            var created = await roles.CreateAsync(request, ct);
            _logger.LogInformation("Role {RoleId} '{RoleName}' created by {Actor} with {Count} permissions",
                created.Id, created.Name, User.Identity?.Name, created.Permissions.Count);
            return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
        }
        catch (Exception ex)
        {
            return Failure(ex, "Creating a role");
        }
    }

    [HttpPut("{id:int}")]
    [Authorize(Policy = "roles.edit")]
    public async Task<IActionResult> Update(int id, [FromBody] RoleEditRequest request, CancellationToken ct)
    {
        try
        {
            var updated = await roles.UpdateAsync(id, request, ct);

            if (updated is null)
                return NotFound();

            _logger.LogInformation("Role {RoleId} '{RoleName}' updated by {Actor} with {Count} permissions",
                id, updated.Name, User.Identity?.Name, updated.Permissions.Count);
            return Ok(updated);
        }
        catch (Exception ex)
        {
            return Failure(ex, $"Updating role {id}");
        }
    }

    [HttpDelete("{id:int}")]
    [Authorize(Policy = "roles.edit")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        try
        {
            var found = await roles.DeleteAsync(id, ct);

            if (!found)
                return NotFound();

            _logger.LogInformation("Role {RoleId} deleted by {Actor}", id, User.Identity?.Name);
            return NoContent();
        }
        catch (Exception ex)
        {
            return Failure(ex, $"Deleting role {id}");
        }
    }
}
