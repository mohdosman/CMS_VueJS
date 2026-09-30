namespace CrisisManagement.Data.Constants;

public static class StoredProcedureConstants
{
    public const string AssessmentSearchRd = "usp_CMS_SearchAssessment_rd";
    public const string QualifiedAssessmentSearchRd = $"{DatabaseConstants.DefaultSchema}.{AssessmentSearchRd}";

    public const string FileSearchRd = "usp_CMS_FileSearch_rd";
    public const string QualifiedFileSearchRd = $"{DatabaseConstants.DefaultSchema}.{FileSearchRd}";

    public const string ServiceSearchRd = "usp_CMS_ServiceSearch_rd";
    public const string QualifiedServiceSearchRd = $"{DatabaseConstants.DefaultSchema}.{ServiceSearchRd}";

    public const string ServiceFileSearchRd = "usp_CMS_ServiceFileSearch_rd";
    public const string QualifiedServiceFileSearchRd = $"{DatabaseConstants.DefaultSchema}.{ServiceFileSearchRd}";

    public const string ServiceFileErrorSearchRd = "usp_CMS_DisplayServiceFileErrors_rd";
    public const string QualifiedServiceFileErrorSearchRd = $"{DatabaseConstants.DefaultSchema}.{ServiceFileErrorSearchRd}";

    public const string SuicideFileSearchRd = "usp_CMS_SuicideFileSearch_rd";
    public const string QualifiedSuicideFileSearchRd = $"{DatabaseConstants.DefaultSchema}.{SuicideFileSearchRd}";

    public const string TempTriggerUpd = $"{DatabaseConstants.DefaultSchema}.usp_CMS_TempTrigger_upd";
}
