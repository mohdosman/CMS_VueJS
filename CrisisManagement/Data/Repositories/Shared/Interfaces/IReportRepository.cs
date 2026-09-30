using CrisisManagement.Data.Models.Identity;

namespace CrisisManagement.Data.Repositories.Interfaces;

// The report definitions (RBS_Report) and the log of report runs.
public interface IReportRepository : IRepository<Report>
{
    // Untracked; the name and description filters are "contains".
    Task<List<Report>> SearchAsync(string? nameContains, string? descriptionContains);

    Task<Report?> GetByKeyAsync(Guid key);

    // Ids and names of every report, for the checks that compare against all of them.
    Task<List<(int Id, string Name)>> GetNamesAsync();

    Task<bool> FileNameExistsAsync(string fileName, int exceptId);

    void AddLog(ReportLog log);
}
