namespace CrisisManagement.Data.Models.SP;

public sealed class UspServiceSearchRd
{
    public int TotalRowCount { get; set; }

    public int ServiceId { get; set; }

    public string? ProviderPatientNo { get; set; }

    public string? LastName { get; set; }

    public string? FirstName { get; set; }

    public string? DOB { get; set; }

    public string? SSN { get; set; }

    public string? ProviderAbbrev { get; set; }

    public string? ServiceCodeAbbrev { get; set; }

    public string? DOSAdmitDate { get; set; }

    public string? DischargeDate { get; set; }
}
