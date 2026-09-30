namespace CrisisManagement.Data.Models.SP;

public sealed class UspServiceFileSearchRd
{
    public int TotalRowCount { get; set; }
    public int ServiceFileId { get; set; }

    public string FileName { get; set; } = string.Empty;

    public string ProviderNPI { get; set; } = string.Empty;

    public int ProviderId { get; set; }

    public DateTime FileCreationDate { get; set; }

    public int? ServiceCount { get; set; }

    public int? ErrorCount { get; set; }

    public int? PostedCount { get; set; }

    public bool IsProcessed { get; set; }

    public bool IsInProcess { get; set; }
    public DateTime CreatedOn { get; set; }
}
