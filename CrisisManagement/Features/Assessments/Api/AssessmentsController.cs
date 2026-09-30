using CrisisManagement.Features.Assessments.Services;
using CrisisManagement.Features.Assessments.ViewModels;
using CrisisManagement.Shared.Api;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CrisisManagement.Features.Assessments.Api;

// Policies: assessments.view / .edit / .delete for the assessments, assessments.fileupload for uploading, and
// assessments.files.view for Display Files (also granted by assessments.fileupload, see PermissionHandler).
// Everything is limited to the caller's providers.
public sealed class AssessmentsController(
    AssessmentSearchService search,
    AssessmentFileService files,
    AssessmentEditorService editor,
    ILogger<AssessmentsController> logger) : BaseApiController(logger)
{
    private readonly ILogger<AssessmentsController> _logger = logger;

    // ---------------------------------------------------------------- Search Assessment

    [HttpGet("providers")]
    [Authorize(Policy = "assessments.view")]
    public async Task<IActionResult> Providers()
    {
        try { return Ok(await search.GetProvidersAsync()); }
        catch (Exception ex) { return Failure(ex, "Loading the provider lookup"); }
    }

    [HttpPost("search")]
    [Authorize(Policy = "assessments.view")]
    public async Task<IActionResult> Search([FromBody] AssessmentSearchRequest request)
    {
        try { return Ok(await search.SearchAsync(request)); }
        catch (Exception ex) { return Failure(ex, "Searching assessments"); }
    }

    // ---------------------------------------------------------------- Enter/Edit Assessment

    [HttpGet("lookups")]
    [Authorize(Policy = "assessments.view")]
    public async Task<IActionResult> Lookups()
    {
        try { return Ok(await editor.GetLookupsAsync()); }
        catch (Exception ex) { return Failure(ex, "Loading the assessment lookups"); }
    }

    // key: "f2f-<id>" or "pa-<id>"; "0" is a blank form for a new assessment.
    [HttpGet("detail/{key}")]
    [Authorize(Policy = "assessments.view")]
    public async Task<IActionResult> Detail(string key)
    {
        try
        {
            if (key == "0") return Ok(new AssessmentEditModel());
            var model = await editor.GetAsync(key);
            return model is null ? NotFound() : Ok(model);
        }
        catch (Exception ex) { return Failure(ex, $"Loading assessment {key}"); }
    }

    [HttpPost]
    [Authorize(Policy = "assessments.edit")]
    public async Task<IActionResult> Create([FromBody] AssessmentEditModel model)
    {
        try
        {
            var saved = await editor.CreateAsync(model);
            _logger.LogInformation("Assessment {Key} created by {Actor}", saved.Key, User.Identity?.Name);
            return Ok(saved);
        }
        catch (Exception ex) { return Failure(ex, "Creating an assessment"); }
    }

    [HttpPut("{key}")]
    [Authorize(Policy = "assessments.edit")]
    public async Task<IActionResult> Update(string key, [FromBody] AssessmentEditModel model)
    {
        try
        {
            var saved = await editor.UpdateAsync(key, model);
            if (saved is null) return NotFound();
            _logger.LogInformation("Assessment {Key} updated by {Actor}", key, User.Identity?.Name);
            return Ok(saved);
        }
        catch (Exception ex) { return Failure(ex, $"Updating assessment {key}"); }
    }

    [HttpDelete("{key}")]
    [Authorize(Policy = "assessments.delete")]
    public async Task<IActionResult> Delete(string key)
    {
        try
        {
            if (!await editor.DeleteAsync(key)) return NotFound();
            _logger.LogInformation("Assessment {Key} deleted by {Actor}", key, User.Identity?.Name);
            return NoContent();
        }
        catch (Exception ex) { return Failure(ex, $"Deleting assessment {key}"); }
    }

    // ---------------------------------------------------------------- Display Files and upload

    [HttpGet("files/providers")]
    [Authorize(Policy = "assessments.files.view")]
    public async Task<IActionResult> FileProviders()
    {
        try { return Ok(await search.GetProvidersAsync()); }
        catch (Exception ex) { return Failure(ex, "Loading the provider lookup for files"); }
    }

    [HttpPost("files/search")]
    [Authorize(Policy = "assessments.files.view")]
    public async Task<IActionResult> SearchFiles([FromBody] AssessmentFileSearchRequest request)
    {
        try { return Ok(await files.SearchAsync(request)); }
        catch (Exception ex) { return Failure(ex, "Searching assessment files"); }
    }

    [HttpGet("files/{id:int}/raw")]
    [Authorize(Policy = "assessments.files.view")]
    public async Task<IActionResult> Raw(int id)
    {
        try
        {
            var file = await files.GetRawAsync(id);
            return file is null ? NotFound() : Ok(file);
        }
        catch (Exception ex) { return Failure(ex, $"Loading assessment file {id}"); }
    }

    [HttpGet("files/{id:int}/errors")]
    [Authorize(Policy = "assessments.files.view")]
    public async Task<IActionResult> Errors(int id)
    {
        try
        {
            var list = await files.GetErrorsAsync(id);
            return list is null ? NotFound() : Ok(list);
        }
        catch (Exception ex) { return Failure(ex, $"Loading the errors of assessment file {id}"); }
    }

    [HttpPost("files")]
    [Authorize(Policy = "assessments.fileupload")]
    [RequestSizeLimit(6 * 1024 * 1024)]
    public async Task<IActionResult> Upload(IFormFile file)
    {
        try
        {
            await using var buffer = new MemoryStream();
            await file.CopyToAsync(buffer);

            var stored = await files.UploadAsync(file.FileName, buffer.ToArray());
            _logger.LogInformation("Assessment file {FileUploadId} '{FileName}' uploaded by {Actor}", stored.Id, stored.FileName, User.Identity?.Name);
            return Ok(stored);
        }
        catch (Exception ex) { return Failure(ex, "Uploading an assessment file"); }
    }
}
