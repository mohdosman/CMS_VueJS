using CrisisManagement.Data.Context;
using CrisisManagement.Data.Models.Identity;
using CrisisManagement.Data.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CrisisManagement.Data.Repositories;

public sealed class ReportRepository(AppDbContext context) : Repository<Report>(context), IReportRepository
{
    public Task<List<Report>> SearchAsync(string? nameContains, string? descriptionContains)
    {
        var q = context.Reports.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(nameContains)) q = q.Where(r => r.ReportName.Contains(nameContains));
        if (!string.IsNullOrWhiteSpace(descriptionContains)) q = q.Where(r => r.Description.Contains(descriptionContains));
        return q.ToListAsync();
    }

    public Task<Report?> GetByKeyAsync(Guid key) => context.Reports.FirstOrDefaultAsync(r => r.ReportKey == key);

    public async Task<List<(int Id, string Name)>> GetNamesAsync() =>
        (await context.Reports.AsNoTracking().Select(r => new { r.ReportId, r.ReportName }).ToListAsync()).Select(r => (r.ReportId, r.ReportName)).ToList();

    public Task<bool> FileNameExistsAsync(string fileName, int exceptId) =>
        context.Reports.AnyAsync(r => r.FileName == fileName && r.ReportId != exceptId);

    public void AddLog(ReportLog log) => context.ReportLogs.Add(log);
}
