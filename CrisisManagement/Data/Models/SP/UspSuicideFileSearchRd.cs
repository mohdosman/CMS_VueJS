namespace CrisisManagement.Data.Models.SP
{
    public sealed class UspSuicideFileSearchRd
    {
        public int TotalRowCount { get; set; }
        public int SuicideFileId { get; set; }
        public string? FileName { get; set; }
        public int? FileSize { get; set; }
        public int? RecordCount { get; set; }
        public DateTime? CreatedOn { get; set; }
    }
}
