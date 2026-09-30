using CMS.Data.Models;

namespace CMS.Data.Models.Identity
{
    public partial class ReportLog : AuditableEntity
    {
        public int ReportLogId { get; set; }
        public int ReportId { get; set; }
        public string ReportName { get; set; } = string.Empty;
        public string SecurityToken { get; set; } = string.Empty;
    }
}
