using CrisisManagement.Features.Services.Services;
using CrisisManagement.Features.Services.ViewModels;
using CrisisManagement.Shared.Api;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CrisisManagement.Features.Services.Api;

// Policies: services.view to search and read, services.enter to enter one (services.edit also grants it), services.edit
// to change, services.delete to delete, services.fileupload to upload and services.files.view for Display Service Files (also granted by
// services.fileupload, see PermissionHandler). Everything is limited to the caller's providers.
public sealed class ServicesController(
    ServiceSearchService search,
    ServiceEditorService editor,
    ServiceFileService files,
    ILogger<ServicesController> logger) : BaseApiController(logger)
{
    private readonly ILogger<ServicesController> _logger = logger;

    // ---------------------------------------------------------------- Manage Service

    [HttpGet("providers")]
    [Authorize(Policy = "services.view")]
    public async Task<IActionResult> Providers()
    {
        try { return Ok(await search.GetProvidersAsync()); }
        catch (Exception ex) { return Failure(ex, "Loading the provider lookup"); }
    }

    [HttpGet("service-codes")]
    [Authorize(Policy = "services.view")]
    public async Task<IActionResult> ServiceCodes()
    {
        try { return Ok(await search.GetServiceCodesAsync()); }
        catch (Exception ex) { return Failure(ex, "Loading the service codes"); }
    }

    [HttpPost("search")]
    [Authorize(Policy = "services.view")]
    public async Task<IActionResult> Search([FromBody] ServiceSearchRequest request)
    {
        try { return Ok(await search.SearchAsync(request)); }
        catch (Exception ex) { return Failure(ex, "Searching services"); }
    }

    // ---------------------------------------------------------------- Enter/Edit Service

    [HttpGet("lookups")]
    [Authorize(Policy = "services.view")]
    public async Task<IActionResult> Lookups()
    {
        try { return Ok(await editor.GetLookupsAsync()); }
        catch (Exception ex) { return Failure(ex, "Loading the service lookups"); }
    }

    [HttpGet("entry-providers")]
    [Authorize(Policy = "services.view")]
    public async Task<IActionResult> EntryProviders()
    {
        try { return Ok(await search.GetProvidersAsync()); }
        catch (Exception ex) { return Failure(ex, "Loading the provider lookup"); }
    }

    // What the caller entered in this browser session (the grid under the Enter Service form).
    [HttpPost("search-current-session")]
    [Authorize(Policy = "services.enter")]
    public async Task<IActionResult> SearchCurrentSession([FromBody] ServiceSearchRequest request)
    {
        try { return Ok(await search.SearchAsync(request, currentSessionOnly: true)); }
        catch (Exception ex) { return Failure(ex, "Searching the services entered in this session"); }
    }

    [HttpGet("existing-patient")]
    [Authorize(Policy = "services.enter")]
    public async Task<IActionResult> ExistingPatient([FromQuery] int providerId, [FromQuery] string? providerPatientNo)
    {
        try { return Ok(await editor.FindExistingPatientsAsync(providerId, providerPatientNo)); }
        catch (Exception ex) { return Failure(ex, "Looking up an existing patient"); }
    }

    [HttpGet("{id:int}")]
    [Authorize(Policy = "services.view")]
    public async Task<IActionResult> Get(int id)
    {
        try
        {
            var model = await editor.GetAsync(id);
            return model is null ? NotFound() : Ok(model);
        }
        catch (Exception ex) { return Failure(ex, $"Loading service {id}"); }
    }

    [HttpPost]
    [Authorize(Policy = "services.enter")]
    public async Task<IActionResult> Create([FromBody] ServiceEditModel model)
    {
        try
        {
            var created = await editor.CreateAsync(model);
            _logger.LogInformation("Service {ServiceId} created by {Actor}", created.Id, User.Identity?.Name);
            return Ok(created);
        }
        catch (Exception ex) { return Failure(ex, "Creating a service"); }
    }

    [HttpPut("{id:int}")]
    [Authorize(Policy = "services.edit")]
    public async Task<IActionResult> Update(int id, [FromBody] ServiceEditModel model)
    {
        try
        {
            var updated = await editor.UpdateAsync(id, model);
            if (updated is null) return NotFound();
            _logger.LogInformation("Service {ServiceId} updated by {Actor}", id, User.Identity?.Name);
            return Ok(updated);
        }
        catch (Exception ex) { return Failure(ex, $"Updating service {id}"); }
    }

    [HttpDelete("{id:int}")]
    [Authorize(Policy = "services.delete")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            if (!await editor.DeleteAsync(id)) return NotFound();
            _logger.LogInformation("Service {ServiceId} deleted by {Actor}", id, User.Identity?.Name);
            return NoContent();
        }
        catch (Exception ex) { return Failure(ex, $"Deleting service {id}"); }
    }

    // ---------------------------------------------------------------- Display Service Files and upload

    [HttpGet("files/providers")]
    [Authorize(Policy = "services.files.view")]
    public async Task<IActionResult> FileProviders()
    {
        try { return Ok(await search.GetProvidersAsync()); }
        catch (Exception ex) { return Failure(ex, "Loading the provider lookup for service files"); }
    }

    [HttpPost("files/search")]
    [Authorize(Policy = "services.files.view")]
    public async Task<IActionResult> SearchFiles([FromBody] ServiceFileSearchRequest request)
    {
        try { return Ok(await files.SearchAsync(request)); }
        catch (Exception ex) { return Failure(ex, "Searching service files"); }
    }

    [HttpGet("files/{id:int}/raw")]
    [Authorize(Policy = "services.files.view")]
    public async Task<IActionResult> Raw(int id)
    {
        try
        {
            var file = await files.GetRawAsync(id);
            return file is null ? NotFound() : Ok(file);
        }
        catch (Exception ex) { return Failure(ex, $"Loading service file {id}"); }
    }

    [HttpPost("files/{id:int}/errors")]
    [Authorize(Policy = "services.files.view")]
    public async Task<IActionResult> Errors(int id, [FromBody] ServiceFileErrorSearchRequest request)
    {
        try
        {
            var list = await files.SearchErrorsAsync(id, request);
            return list is null ? NotFound() : Ok(list);
        }
        catch (Exception ex) { return Failure(ex, $"Loading the errors of service file {id}"); }
    }

    [HttpPost("files")]
    [Authorize(Policy = "services.fileupload")]
    [RequestSizeLimit(6 * 1024 * 1024)]
    public async Task<IActionResult> Upload(IFormFile file)
    {
        try
        {
            await using var buffer = new MemoryStream();
            await file.CopyToAsync(buffer);

            var stored = await files.UploadAsync(file.FileName, buffer.ToArray());
            _logger.LogInformation("Service file {ServiceFileId} '{FileName}' uploaded by {Actor}", stored.Id, stored.FileName, User.Identity?.Name);
            return Ok(stored);
        }
        catch (Exception ex) { return Failure(ex, "Uploading a service file"); }
    }
}
