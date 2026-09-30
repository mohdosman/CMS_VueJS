using CrisisManagement.Features.PublicFiles.Services;
using CrisisManagement.Features.PublicFiles.ViewModels;
using CrisisManagement.Shared.Api;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CrisisManagement.Features.PublicFiles.Api;

// Help documents. "help" and "download" are open to any signed-in user (the Help dialog) and only ever expose
// active help documents, never per-user agreements. "search", upload and delete are the Public Files screen,
// under publicfiles.view / publicfiles.edit (the Blazor CMS wrongly used the Services permissions there).
public sealed class PublicFilesController(IPublicFilesService publicFiles, ILogger<PublicFilesController> logger)
    : BaseApiController(logger)
{
    private readonly ILogger<PublicFilesController> _logger = logger;

    [HttpGet("help")]
    public async Task<IActionResult> Help(CancellationToken ct)
    {
        try
        {
            return Ok(await publicFiles.GetHelpFilesAsync(ct));
        }
        catch (Exception ex)
        {
            return Failure(ex, "Listing the help files");
        }
    }

    // Streamed as a real file so the browser handles the download; no base64 round trip.
    [HttpGet("{id:int}/download")]
    public async Task<IActionResult> Download(int id, CancellationToken ct)
    {
        try
        {
            var file = await publicFiles.GetHelpFileAsync(id, ct);

            if (file is null)
                return NotFound();

            return File(file.Content, file.MimeType, file.FileName);
        }
        catch (Exception ex)
        {
            return Failure(ex, $"Downloading help file {id}");
        }
    }

    [HttpPost("search")]
    [Authorize(Policy = "publicfiles.view")]
    public async Task<IActionResult> Search([FromBody] PublicFileSearchRequest request)
    {
        try
        {
            return Ok(await publicFiles.SearchAsync(request));
        }
        catch (Exception ex)
        {
            return Failure(ex, "Searching public files");
        }
    }

    [HttpPost]
    [Authorize(Policy = "publicfiles.edit")]
    [RequestSizeLimit(6 * 1024 * 1024)]
    public async Task<IActionResult> Upload(IFormFile file)
    {
        try
        {
            await using var buffer = new MemoryStream();
            await file.CopyToAsync(buffer);

            var stored = await publicFiles.UploadAsync(file.FileName, buffer.ToArray());
            _logger.LogInformation("Public file {DocumentId} '{FileName}' uploaded by {Actor}", stored.Id, stored.FileName, User.Identity?.Name);
            return Ok(stored);
        }
        catch (Exception ex)
        {
            return Failure(ex, "Uploading a public file");
        }
    }

    [HttpDelete("{id:int}")]
    [Authorize(Policy = "publicfiles.edit")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var found = await publicFiles.DeleteAsync(id);

            if (!found)
                return NotFound();

            _logger.LogInformation("Public file {DocumentId} deleted by {Actor}", id, User.Identity?.Name);
            return NoContent();
        }
        catch (Exception ex)
        {
            return Failure(ex, $"Deleting public file {id}");
        }
    }
}
