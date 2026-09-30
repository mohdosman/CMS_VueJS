using CrisisManagement.Features.Suicides.Services;
using CrisisManagement.Features.Suicides.ViewModels;
using CrisisManagement.Shared.Api;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CrisisManagement.Features.Suicides.Api;

// Policies: suicides.view to list, view and download the files, suicides.edit to upload one. Not limited by provider.
public sealed class SuicidesController(SuicideFileService files, ILogger<SuicidesController> logger) : BaseApiController(logger)
{
    private const string XlsxType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
    private readonly ILogger<SuicidesController> _logger = logger;

    [HttpPost("files/search")]
    [Authorize(Policy = "suicides.view")]
    public async Task<IActionResult> Search([FromBody] SuicideFileSearchRequest request)
    {
        try { return Ok(await files.SearchAsync(request)); }
        catch (Exception ex) { return Failure(ex, "Searching suicide files"); }
    }

    [HttpPost("files/{id:int}/imports")]
    [Authorize(Policy = "suicides.view")]
    public async Task<IActionResult> Imports(int id, [FromBody] SuicideImportSearchRequest request)
    {
        try { return Ok(await files.SearchImportsAsync(id, request)); }
        catch (Exception ex) { return Failure(ex, $"Loading the records of suicide file {id}"); }
    }

    [HttpGet("files/{id:int}/download")]
    [Authorize(Policy = "suicides.view")]
    public async Task<IActionResult> Download(int id)
    {
        try
        {
            var file = await files.DownloadAsync(id);
            return file is null ? NotFound() : File(file.Content, XlsxType, file.FileName);
        }
        catch (Exception ex) { return Failure(ex, $"Downloading suicide file {id}"); }
    }

    [HttpPost("files")]
    [Authorize(Policy = "suicides.edit")]
    [RequestSizeLimit(6 * 1024 * 1024)]
    public async Task<IActionResult> Upload(IFormFile file)
    {
        try
        {
            await using var buffer = new MemoryStream();
            await file.CopyToAsync(buffer);

            var stored = await files.UploadAsync(file.FileName, buffer.ToArray());
            _logger.LogInformation("Suicide file {SuicideFileId} '{FileName}' uploaded by {Actor}", stored.Id, stored.FileName, User.Identity?.Name);
            return Ok(stored);
        }
        catch (Exception ex) { return Failure(ex, "Uploading a suicide file"); }
    }
}
