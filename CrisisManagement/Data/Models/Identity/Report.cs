using CrisisManagement.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrisisManagement.Data.Models.Identity
{
    public partial class Report : AuditableEntity
    {
        public int ReportId { get; set; }
        public Guid ReportKey { get; set; } = Guid.NewGuid();
        public string ReportName { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string ExportOption { get; set; } = string.Empty;
    }
}
