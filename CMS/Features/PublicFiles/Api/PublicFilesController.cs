using CMS.Features.PublicFiles.Services;
using CMS.Features.PublicFiles.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CMS.Features.PublicFiles.Api;

// Read side of the Blazor CMS PublicFilesController "help-files" + download. Any signed-in user may
// use it; only active help documents are exposed, never per-user agreements.
[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class PublicFilesController(IPublicFilesService publicFiles) : ControllerBase
{
    [HttpGet("help")]
    public Task<List<HelpFileViewModel>> Help(CancellationToken ct) => publicFiles.GetHelpFilesAsync(ct);

    // Streamed as a real file so the browser handles the download; no base64 round trip.
    [HttpGet("{id:int}/download")]
    public async Task<IActionResult> Download(int id, CancellationToken ct)
    {
        var file = await publicFiles.GetHelpFileAsync(id, ct);
        return file is null ? NotFound() : File(file.Content, file.MimeType, file.FileName);
    }
}
