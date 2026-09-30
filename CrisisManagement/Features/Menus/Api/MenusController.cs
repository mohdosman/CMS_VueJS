using CrisisManagement.Features.Menus.ViewModels;
using CrisisManagement.Shared.Api;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CrisisManagement.Features.Menus.Api;

// Policy "menus.view" is also satisfied by "menus.edit" (see PermissionHandler). The Blazor CMS had no policy here at all.
[Authorize(Policy = "menus.view")]
public sealed class MenusController(MenuAdminService menus, ILogger<MenusController> logger) : BaseApiController(logger)
{
    private readonly ILogger<MenusController> _logger = logger;

    // ---------------------------------------------------------------- menus

    [HttpGet]
    public async Task<IActionResult> List()
    {
        try { return Ok(await menus.ListAsync()); }
        catch (Exception ex) { return Failure(ex, "Listing menu items"); }
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Get(int id)
    {
        try
        {
            var detail = await menus.GetAsync(id);
            return detail is null ? NotFound() : Ok(detail);
        }
        catch (Exception ex) { return Failure(ex, $"Loading menu item {id}"); }
    }

    [HttpGet("parents")]
    public async Task<IActionResult> Parents(int? excludeId)
    {
        try { return Ok(await menus.ParentsAsync(excludeId)); }
        catch (Exception ex) { return Failure(ex, "Loading the parent choices"); }
    }

    [HttpPost]
    [Authorize(Policy = "menus.edit")]
    public async Task<IActionResult> Create([FromBody] MenuEditRequest request)
    {
        try
        {
            var created = await menus.CreateAsync(request);
            _logger.LogInformation("Menu item {MenuItemId} '{Name}' created by {Actor}", created.Id, created.Name, User.Identity?.Name);
            return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
        }
        catch (Exception ex) { return Failure(ex, "Creating a menu item"); }
    }

    [HttpPut("{id:int}")]
    [Authorize(Policy = "menus.edit")]
    public async Task<IActionResult> Update(int id, [FromBody] MenuEditRequest request)
    {
        try
        {
            var updated = await menus.UpdateAsync(id, request);

            if (updated is null)
                return NotFound();

            _logger.LogInformation("Menu item {MenuItemId} updated by {Actor}", id, User.Identity?.Name);
            return Ok(updated);
        }
        catch (Exception ex) { return Failure(ex, $"Updating menu item {id}"); }
    }

    [HttpDelete("{id:int}")]
    [Authorize(Policy = "menus.edit")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var found = await menus.DeleteAsync(id);

            if (!found)
                return NotFound();

            _logger.LogInformation("Menu item {MenuItemId} deleted by {Actor}", id, User.Identity?.Name);
            return NoContent();
        }
        catch (Exception ex) { return Failure(ex, $"Deleting menu item {id}"); }
    }

    // ---------------------------------------------------------------- permissions

    [HttpGet("{id:int}/permissions")]
    public async Task<IActionResult> Permissions(int id)
    {
        try
        {
            var list = await menus.PermissionsAsync(id);
            return list is null ? NotFound() : Ok(list);
        }
        catch (Exception ex) { return Failure(ex, $"Listing the permissions of menu item {id}"); }
    }

    [HttpPost("permissions")]
    [Authorize(Policy = "menus.edit")]
    public async Task<IActionResult> CreatePermission([FromBody] PermissionEditRequest request)
    {
        try
        {
            var created = await menus.CreatePermissionAsync(request);
            _logger.LogInformation("Permission {PermissionId} '{Value}' created on menu item {MenuItemId} by {Actor}", created.Id, created.Value, request.MenuId, User.Identity?.Name);
            return Ok(created);
        }
        catch (Exception ex) { return Failure(ex, "Creating a permission"); }
    }

    [HttpPut("permissions/{id:int}")]
    [Authorize(Policy = "menus.edit")]
    public async Task<IActionResult> UpdatePermission(int id, [FromBody] PermissionEditRequest request)
    {
        try
        {
            var updated = await menus.UpdatePermissionAsync(id, request);

            if (updated is null)
                return NotFound();

            _logger.LogInformation("Permission {PermissionId} updated by {Actor}", id, User.Identity?.Name);
            return Ok(updated);
        }
        catch (Exception ex) { return Failure(ex, $"Updating permission {id}"); }
    }

    [HttpDelete("permissions/{id:int}")]
    [Authorize(Policy = "menus.edit")]
    public async Task<IActionResult> DeletePermission(int id)
    {
        try
        {
            var found = await menus.DeletePermissionAsync(id);

            if (!found)
                return NotFound();

            _logger.LogInformation("Permission {PermissionId} deleted by {Actor}", id, User.Identity?.Name);
            return NoContent();
        }
        catch (Exception ex) { return Failure(ex, $"Deleting permission {id}"); }
    }

    // ---------------------------------------------------------------- permission groups

    [HttpGet("groups")]
    public async Task<IActionResult> Groups()
    {
        try { return Ok(await menus.GroupsAsync()); }
        catch (Exception ex) { return Failure(ex, "Listing the permission groups"); }
    }

    [HttpPost("groups")]
    [Authorize(Policy = "menus.edit")]
    public async Task<IActionResult> CreateGroup([FromBody] GroupEditRequest request)
    {
        try
        {
            var created = await menus.CreateGroupAsync(request);
            _logger.LogInformation("Permission group {GroupId} '{Name}' created by {Actor}", created.Id, created.Name, User.Identity?.Name);
            return Ok(created);
        }
        catch (Exception ex) { return Failure(ex, "Creating a permission group"); }
    }

    [HttpPut("groups/{id:int}")]
    [Authorize(Policy = "menus.edit")]
    public async Task<IActionResult> UpdateGroup(int id, [FromBody] GroupEditRequest request)
    {
        try
        {
            var updated = await menus.UpdateGroupAsync(id, request);

            if (updated is null)
                return NotFound();

            _logger.LogInformation("Permission group {GroupId} updated by {Actor}", id, User.Identity?.Name);
            return Ok(updated);
        }
        catch (Exception ex) { return Failure(ex, $"Updating permission group {id}"); }
    }
}
