using CrisisManagement.Features.Providers.Services;
using CrisisManagement.Features.Providers.ViewModels;
using CrisisManagement.Shared.Api;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CrisisManagement.Features.Providers.Api;

// Policy "providers.view" is also satisfied by "providers.edit" (see PermissionHandler).
[Authorize(Policy = "providers.view")]
public sealed class ProvidersController(ProviderService providers, ILogger<ProvidersController> logger) : BaseApiController(logger)
{
    private readonly ILogger<ProvidersController> _logger = logger;

    [HttpPost("search")]
    public async Task<IActionResult> Search([FromBody] ProviderSearchRequest request)
    {
        try
        {
            return Ok(await providers.SearchAsync(request));
        }
        catch (Exception ex)
        {
            return Failure(ex, "Searching providers");
        }
    }

    // States and counties for the address dropdowns.
    [HttpGet("lookups")]
    public async Task<IActionResult> Lookups()
    {
        try
        {
            return Ok(await providers.GetLookupsAsync());
        }
        catch (Exception ex)
        {
            return Failure(ex, "Loading the provider lookups");
        }
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Get(int id)
    {
        try
        {
            var (detail, allowed) = await providers.GetAsync(id);

            if (!allowed)
                return Forbid();

            return detail is null ? NotFound() : Ok(detail);
        }
        catch (Exception ex)
        {
            return Failure(ex, $"Loading provider {id}");
        }
    }

    [HttpPost]
    [Authorize(Policy = "providers.edit")]
    public async Task<IActionResult> Create([FromBody] ProviderEditRequest request)
    {
        try
        {
            var created = await providers.CreateAsync(request);
            _logger.LogInformation("Provider {ProviderId} '{Name}' created by {Actor}", created.Id, created.Name, User.Identity?.Name);
            return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
        }
        catch (Exception ex)
        {
            return Failure(ex, "Creating a provider");
        }
    }

    [HttpPut("{id:int}")]
    [Authorize(Policy = "providers.edit")]
    public async Task<IActionResult> Update(int id, [FromBody] ProviderEditRequest request)
    {
        try
        {
            var updated = await providers.UpdateAsync(id, request);

            if (updated is null)
                return NotFound();

            _logger.LogInformation("Provider {ProviderId} updated by {Actor}", id, User.Identity?.Name);
            return Ok(updated);
        }
        catch (Exception ex)
        {
            return Failure(ex, $"Updating provider {id}");
        }
    }

    [HttpDelete("{id:int}")]
    [Authorize(Policy = "providers.edit")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var found = await providers.DeleteAsync(id);

            if (!found)
                return NotFound();

            _logger.LogInformation("Provider {ProviderId} deleted by {Actor}", id, User.Identity?.Name);
            return NoContent();
        }
        catch (Exception ex)
        {
            return Failure(ex, $"Deleting provider {id}");
        }
    }
}
