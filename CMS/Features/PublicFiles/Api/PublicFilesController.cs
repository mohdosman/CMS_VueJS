using CMS.Features.PublicFiles.Services;
using CMS.Shared.Api;
using Microsoft.AspNetCore.Mvc;

namespace CMS.Features.PublicFiles.Api;

// Read side of the Blazor CMS PublicFilesController "help-files" + download. Any signed-in user may
// use it; only active help documents are exposed, never per-user agreements.
public sealed class PublicFilesController(IPublicFilesService publicFiles, ILogger<PublicFilesController> logger)
    : BaseApiController(logger)
{
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
}
