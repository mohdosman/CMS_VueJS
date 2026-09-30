using CMS.Data.Context;
using CMS.Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CMS.Features.PublicFiles;

public sealed record HelpFile(int Id, string FileName, DateTime CreatedOn, int FileSize);

// Read side of the Blazor CMS PublicFilesController "help-files" + download. Any signed-in user may
// use it; only active help documents are exposed, never per-user agreements.
[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class PublicFilesController(AppDbContext db) : ControllerBase
{
    private IQueryable<Document> HelpDocuments =>
        db.Documents.AsNoTracking().Where(d => d.DocumentTypeId == Document.HelpTypeId && d.UserId == null && d.IsActive);

    [HttpGet("help")]
    public async Task<List<HelpFile>> Help(CancellationToken ct) =>
        await HelpDocuments.OrderByDescending(d => d.CreatedOn)
            .Select(d => new HelpFile(d.DocumentId, d.FileName, d.CreatedOn, d.FileContent.Length))
            .ToListAsync(ct);

    // Streamed as a real file so the browser handles the download; no base64 round trip.
    [HttpGet("{id:int}/download")]
    public async Task<IActionResult> Download(int id, CancellationToken ct)
    {
        var d = await HelpDocuments.Where(x => x.DocumentId == id).FirstOrDefaultAsync(ct);
        return d is null ? NotFound() : File(d.FileContent, d.MIMEType, d.FileName);
    }
}
