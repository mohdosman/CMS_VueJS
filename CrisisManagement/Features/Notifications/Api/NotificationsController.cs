using CrisisManagement.Features.Notifications.Services;
using CrisisManagement.Features.Notifications.ViewModels;
using CrisisManagement.Shared.Api;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CrisisManagement.Features.Notifications.Api;

// Policy "notifications.view" is also satisfied by "notifications.edit" (see PermissionHandler).
[Authorize(Policy = "notifications.view")]
public sealed class NotificationsController(NotificationService notifications, ILogger<NotificationsController> logger) : BaseApiController(logger)
{
    private readonly ILogger<NotificationsController> _logger = logger;

    [HttpPost("search")]
    public async Task<IActionResult> Search([FromBody] NotificationSearchRequest request)
    {
        try
        {
            return Ok(await notifications.SearchAsync(request));
        }
        catch (Exception ex)
        {
            return Failure(ex, "Searching notifications");
        }
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Get(int id)
    {
        try
        {
            var detail = await notifications.GetAsync(id);
            return detail is null ? NotFound() : Ok(detail);
        }
        catch (Exception ex)
        {
            return Failure(ex, $"Loading notification {id}");
        }
    }

    [HttpPost]
    [Authorize(Policy = "notifications.edit")]
    public async Task<IActionResult> Create([FromBody] NotificationEditRequest request)
    {
        try
        {
            var created = await notifications.CreateAsync(request);
            _logger.LogInformation("Notification {NotificationId} created by {Actor}", created.Id, User.Identity?.Name);
            return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
        }
        catch (Exception ex)
        {
            return Failure(ex, "Creating a notification");
        }
    }

    [HttpPut("{id:int}")]
    [Authorize(Policy = "notifications.edit")]
    public async Task<IActionResult> Update(int id, [FromBody] NotificationEditRequest request)
    {
        try
        {
            var updated = await notifications.UpdateAsync(id, request);

            if (updated is null)
                return NotFound();

            _logger.LogInformation("Notification {NotificationId} updated by {Actor}", id, User.Identity?.Name);
            return Ok(updated);
        }
        catch (Exception ex)
        {
            return Failure(ex, $"Updating notification {id}");
        }
    }

    [HttpDelete("{id:int}")]
    [Authorize(Policy = "notifications.edit")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var found = await notifications.DeleteAsync(id);

            if (!found)
                return NotFound();

            _logger.LogInformation("Notification {NotificationId} deleted by {Actor}", id, User.Identity?.Name);
            return NoContent();
        }
        catch (Exception ex)
        {
            return Failure(ex, $"Deleting notification {id}");
        }
    }
}
