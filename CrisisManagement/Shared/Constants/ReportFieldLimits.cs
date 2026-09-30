namespace CrisisManagement.Shared.Constants;

/// <summary>
/// Column lengths for a report's fields. The database is the authority, so the EF configuration is
/// built from these too: a validator that allows more than the column holds fails at SQL with a
/// truncation error instead of a message on the field.
/// </summary>
public static class ReportFieldLimits
{
    /// <summary>RBS_Report.ReportName, varchar(50).</summary>
    public const int MaxReportNameLength = 50;

    /// <summary>RBS_Report.FileName, varchar(255).</summary>
    public const int MaxFileNameLength = 255;

    /// <summary>RBS_Report.Description, varchar(255).</summary>
    public const int MaxDescriptionLength = 255;

    /// <summary>RBS_Report.ExportOption, varchar(50).</summary>
    public const int MaxExportOptionLength = 50;
}
