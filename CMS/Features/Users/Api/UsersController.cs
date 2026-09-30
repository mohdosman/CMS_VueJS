using CMS.Features.Users.Services;
using CMS.Features.Users.ViewModels;
using CMS.Shared.Api;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CMS.Features.Users.Api;

// Policy "users.view" is also satisfied by "users.edit" (see PermissionHandler).
[Authorize(Policy = "users.view")]
public sealed class UsersController(
    UserService users,
    UserDocumentService documents,
    ILogger<UsersController> logger) : BaseApiController(logger)
{
    private readonly ILogger<UsersController> _logger = logger;

    [HttpPost("search")]
    public async Task<IActionResult> Search([FromBody] UserSearchRequest request, CancellationToken ct)
    {
        try
        {
            var result = await users.SearchAsync(request, ct);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return Failure(ex, "Searching users");
        }
    }

    [HttpGet("{key:guid}")]
    public async Task<IActionResult> Get(Guid key, CancellationToken ct)
    {
        try
        {
            var (detail, allowed) = await users.GetAsync(key, ct);

            if (!allowed)
                return Forbid();

            if (detail is null)
                return NotFound();

            return Ok(detail);
        }
        catch (Exception ex)
        {
            return Failure(ex, $"Loading user {key}");
        }
    }

    [HttpGet("roles")]
    public async Task<IActionResult> Roles(CancellationToken ct)
    {
        try
        {
            return Ok(await users.GetRolesAsync(ct));
        }
        catch (Exception ex)
        {
            return Failure(ex, "Loading the role lookup");
        }
    }

    [HttpGet("providers")]
    public async Task<IActionResult> Providers(CancellationToken ct)
    {
        try
        {
            return Ok(await users.GetProvidersAsync(ct));
        }
        catch (Exception ex)
        {
            return Failure(ex, "Loading the provider lookup");
        }
    }

    [HttpGet("policy")]
    public IActionResult Policy()
    {
        try
        {
            return Ok(users.GetPolicy());
        }
        catch (Exception ex)
        {
            return Failure(ex, "Loading the password and user ID policy");
        }
    }

    [HttpPost]
    [Authorize(Policy = "users.edit")]
    public async Task<IActionResult> Create([FromBody] UserEditRequest request, CancellationToken ct)
    {
        try
        {
            var created = await users.CreateAsync(request, ct);
            _logger.LogInformation("User {UserKey} created by {Actor}", created.UserKey, User.Identity?.Name);
            return CreatedAtAction(nameof(Get), new { key = created.UserKey }, created);
        }
        catch (Exception ex)
        {
            return Failure(ex, "Creating a user");
        }
    }

    [HttpPut("{key:guid}")]
    [Authorize(Policy = "users.edit")]
    public async Task<IActionResult> Update(Guid key, [FromBody] UserEditRequest request, CancellationToken ct)
    {
        try
        {
            var updated = await users.UpdateAsync(key, request, ct);

            if (updated is null)
                return NotFound();

            _logger.LogInformation("User {UserKey} updated by {Actor}", key, User.Identity?.Name);
            return Ok(updated);
        }
        catch (Exception ex)
        {
            return Failure(ex, $"Updating user {key}");
        }
    }

    [HttpPost("{key:guid}/password")]
    [Authorize(Policy = "users.edit")]
    public async Task<IActionResult> SetPassword(Guid key, [FromBody] SetPasswordRequest request, CancellationToken ct)
    {
        try
        {
            var found = await users.SetPasswordAsync(key, request, ct);

            if (!found)
                return NotFound();

            // Never log the password itself.
            _logger.LogInformation("Password of user {UserKey} set by {Actor}", key, User.Identity?.Name);
            return NoContent();
        }
        catch (Exception ex)
        {
            return Failure(ex, $"Setting the password of user {key}");
        }
    }

    [HttpPost("{key:guid}/mfa/reset")]
    [Authorize(Policy = "users.edit")]
    public async Task<IActionResult> ResetMfa(Guid key, CancellationToken ct)
    {
        try
        {
            var found = await users.ResetMfaAsync(key, ct);

            if (!found)
                return NotFound();

            _logger.LogInformation("MFA of user {UserKey} reset by {Actor}", key, User.Identity?.Name);
            return NoContent();
        }
        catch (Exception ex)
        {
            return Failure(ex, $"Resetting MFA of user {key}");
        }
    }

    [HttpDelete("{key:guid}")]
    [Authorize(Policy = "users.edit")]
    public async Task<IActionResult> Delete(Guid key, CancellationToken ct)
    {
        try
        {
            var found = await users.DeleteAsync(key, ct);

            if (!found)
                return NotFound();

            _logger.LogInformation("User {UserKey} deleted by {Actor}", key, User.Identity?.Name);
            return NoContent();
        }
        catch (Exception ex)
        {
            return Failure(ex, $"Deleting user {key}");
        }
    }

    // ---------------------------------------------------------------- end user agreements
    // Reading follows users.view, changing follows users.edit.

    [HttpGet("{key:guid}/documents")]
    public async Task<IActionResult> Documents(Guid key)
    {
        try
        {
            var list = await documents.ListAsync(key);
            return list is null ? NotFound() : Ok(list);
        }
        catch (Exception ex)
        {
            return Failure(ex, $"Listing the agreements of user {key}");
        }
    }

    [HttpPost("{key:guid}/documents")]
    [Authorize(Policy = "users.edit")]
    [RequestSizeLimit(11 * 1024 * 1024)]
    public async Task<IActionResult> UploadDocument(Guid key, IFormFile file)
    {
        try
        {
            await using var buffer = new MemoryStream();
            await file.CopyToAsync(buffer);

            var stored = await documents.UploadAsync(key, file.FileName, buffer.ToArray());

            if (stored is null)
                return NotFound();

            _logger.LogInformation("Agreement {DocumentId} uploaded for user {UserKey} by {Actor}", stored.DocumentId, key, User.Identity?.Name);
            return Ok(stored);
        }
        catch (Exception ex)
        {
            return Failure(ex, $"Uploading an agreement for user {key}");
        }
    }

    [HttpGet("{key:guid}/documents/{documentId:int}/download")]
    public async Task<IActionResult> DownloadDocument(Guid key, int documentId)
    {
        try
        {
            var document = await documents.DownloadAsync(key, documentId);

            if (document is null)
                return NotFound();

            return File(document.FileContent, document.MIMEType, document.FileName);
        }
        catch (Exception ex)
        {
            return Failure(ex, $"Downloading agreement {documentId} of user {key}");
        }
    }

    [HttpDelete("{key:guid}/documents/{documentId:int}")]
    [Authorize(Policy = "users.edit")]
    public async Task<IActionResult> DeleteDocument(Guid key, int documentId)
    {
        try
        {
            var found = await documents.DeleteAsync(key, documentId);

            if (!found)
                return NotFound();

            _logger.LogInformation("Agreement {DocumentId} of user {UserKey} deleted by {Actor}", documentId, key, User.Identity?.Name);
            return NoContent();
        }
        catch (Exception ex)
        {
            return Failure(ex, $"Deleting agreement {documentId} of user {key}");
        }
    }
}
