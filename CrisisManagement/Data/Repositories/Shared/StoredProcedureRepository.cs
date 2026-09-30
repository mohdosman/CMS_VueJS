using System.Data;
using CrisisManagement.Data.Context;
using CrisisManagement.Data.Repositories.Interfaces;
using CrisisManagement.Data.StoredProcedures;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace CrisisManagement.Data.Repositories;

public sealed class StoredProcedureRepository(AppDbContext context) : IStoredProcedureRepository
{
    public async Task<(List<TRow> Rows, int Total)> SearchAsync<TRow>(string procedure, SearchPreferences preferences, int page, int size, string sort, Func<TRow, int> totalOf)
    {
        var returnCode = new SqlParameter("@p4", SqlDbType.Int) { Direction = ParameterDirection.Output };
        var errorMessage = new SqlParameter("@p5", SqlDbType.VarChar, 1024) { Direction = ParameterDirection.Output };

        // The procedure name comes from StoredProcedureConstants, never from a caller.
        var rows = await context.Database.SqlQueryRaw<TRow>(
            $"EXEC {procedure} @i_PreferenceXML = @p0, @i_StartRowIndex = @p1, @i_NumRows = @p2, @i_sortExpression = @p3, @o_return_code = @p4 OUTPUT, @o_error_msg = @p5 OUTPUT",
            new SqlParameter("@p0", SqlDbType.Xml) { Value = preferences.ToXml() },
            new SqlParameter("@p1", SqlDbType.Int) { Value = page - 1 },   // the procedures take a page INDEX
            new SqlParameter("@p2", SqlDbType.Int) { Value = size },
            new SqlParameter("@p3", SqlDbType.VarChar, 50) { Value = sort },
            returnCode, errorMessage).ToListAsync();

        if (Convert.ToInt32(returnCode.Value is DBNull ? 0 : returnCode.Value) != 0)
            throw new InvalidOperationException(Convert.ToString(errorMessage.Value) is { Length: > 0 } m ? m : $"{procedure} failed.");

        return (rows, rows.Count == 0 ? 0 : totalOf(rows[0]));
    }

    public Task<int> ExecuteAsync(string sql, params object[] parameters) => context.Database.ExecuteSqlRawAsync(sql, parameters);
}
