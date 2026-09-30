namespace CMS.Data.Models.SP;

/// <summary>
/// Result row returned by dbo.usp_CMS_FileSearch_rd.
/// </summary>
public sealed class UspFileSearchRd
{
    public int TotalRowCount { get; set; }
    public int FileUploadId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public bool IsProcessed { get; set; }
    public int PATotalCount { get; set; }
    public int PACount { get; set; }
    public int PAErrorCount { get; set; }
    public int F2FTotalCount { get; set; }
    public int F2FCount { get; set; }
    public int F2FErrorCount { get; set; }
    public DateTime CreatedOn { get; set; }
}
