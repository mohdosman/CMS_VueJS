namespace CrisisManagement.Data.Models.SP;

public sealed class UspServiceFileErrorSearchRd
{
    public int TotalRowCount { get; set; }
    public int ServiceFileErrorId { get; set; }
    public int ServiceFileId { get; set; }
    public int ServiceFileImportId { get; set; }
    public string? SSN { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? DOB { get; set; }
    public string? ServiceCode { get; set; }
    public string? DOSAdmitDate { get; set; }
    public int ServiceFileErrorCodeId { get; set; }
    public string? ServiceFileErrorCodeDescription { get; set; }
}
