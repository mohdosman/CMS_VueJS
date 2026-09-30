using CrisisManagement.Data.StoredProcedures;

namespace CrisisManagement.Data.Repositories.Interfaces;

// Runs the legacy paged search procedures (usp_CMS_*_rd). They share one signature: a preference XML, a page index, a
// page size, a sort expression and two output parameters; the first result column is TotalRowCount.
public interface IStoredProcedureRepository
{
    // page is 1-based. sort must come from a whitelist in the caller: the procedure places it in ORDER BY as it is.
    Task<(List<TRow> Rows, int Total)> SearchAsync<TRow>(string procedure, SearchPreferences preferences, int page, int size, string sort, Func<TRow, int> totalOf);

    // Some procedures (delete audit, imports) are called by name with plain parameters.
    Task<int> ExecuteAsync(string sql, params object[] parameters);
}
