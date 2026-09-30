using CrisisManagement.Features.Reports.Services;
using CrisisManagement.Features.Reports.ViewModels;
using CrisisManagement.Shared.Api;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CrisisManagement.Features.Reports.Api;

// Listing, reading and running need only a signed-in user: what they can see is decided per report (reports.view.<report>),
// and most roles hold only those, not the general reports.view. Adding, changing and deleting need reports.edit and, for
// an existing report, reports.manage.<report>. See ReportService.
public sealed class ReportsController(ReportService reports, ILogger<ReportsController> logger) : BaseApiController(logger)
{
    private readonly ILogger<ReportsController> _logger = logger;

    [HttpPost("search")]
    [Authorize]
    public async Task<IActionResult> Search([FromBody] ReportSearchRequest request)
    {
        try { return Ok(await reports.SearchAsync(request)); }
        catch (Exception ex) { return Failure(ex, "Searching reports"); }
    }

    // Report files on the server that have no report defined yet.
    [HttpGet("available")]
    [Authorize(Policy = "reports.edit")]
    public async Task<IActionResult> Available()
    {
        try { return Ok(await reports.GetAvailableAsync()); }
        catch (Exception ex) { return Failure(ex, "Listing the available report files"); }
    }

    [HttpGet("{id:int}")]
    [Authorize]
    public async Task<IActionResult> Get(int id)
    {
        try
        {
            var report = await reports.GetAsync(id);
            return report is null ? NotFound() : Ok(report);
        }
        catch (Exception ex) { return Failure(ex, $"Loading report {id}"); }
    }

    [HttpPost]
    [Authorize(Policy = "reports.edit")]
    public async Task<IActionResult> Create([FromBody] ReportEditModel model)
    {
        try
        {
            var created = await reports.CreateAsync(model);
            _logger.LogInformation("Report {ReportId} '{ReportName}' created by {Actor}", created.Id, created.ReportName, User.Identity?.Name);
            return Ok(created);
        }
        catch (Exception ex) { return Failure(ex, "Creating a report"); }
    }

    [HttpPut("{id:int}")]
    [Authorize(Policy = "reports.edit")]
    public async Task<IActionResult> Update(int id, [FromBody] ReportEditModel model)
    {
        try
        {
            var updated = await reports.UpdateAsync(id, model);
            if (updated is null) return NotFound();
            _logger.LogInformation("Report {ReportId} updated by {Actor}", id, User.Identity?.Name);
            return Ok(updated);
        }
        catch (Exception ex) { return Failure(ex, $"Updating report {id}"); }
    }

    [HttpDelete("{id:int}")]
    [Authorize(Policy = "reports.edit")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            if (!await reports.DeleteAsync(id)) return NotFound();
            _logger.LogInformation("Report {ReportId} deleted by {Actor}", id, User.Identity?.Name);
            return NoContent();
        }
        catch (Exception ex) { return Failure(ex, $"Deleting report {id}"); }
    }

    // Runs the report: signs the token, logs the run and answers with the link the browser window opens. A POST, because it
    // writes the log.
    [HttpPost("{reportKey:guid}/run")]
    [Authorize]
    public async Task<IActionResult> Run(Guid reportKey)
    {
        try { return Ok(await reports.RunAsync(reportKey, Url.Content("~/api/reports/open/"))); }
        catch (Exception ex) { return Failure(ex, $"Running report {reportKey}"); }
    }

    // The window's page: posts the signed token to the report server. The link works once, for the user it was made for.
    [HttpGet("open/{linkId}")]
    [Authorize]
    public IActionResult Open(string linkId)
    {
        Response.Headers.CacheControl = "no-store";
        Response.Headers["Referrer-Policy"] = "no-referrer";
        var page = reports.ConsumeOpenLink(linkId);
        if (page is null)
            return new ContentResult { StatusCode = 400, ContentType = "text/plain; charset=utf-8", Content = "Invalid or expired link. Please try running the report again." };
        return Content(page, "text/html", System.Text.Encoding.UTF8);
    }
}
