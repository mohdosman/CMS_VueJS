using CrisisManagement.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrisisManagement.Data.Models.Identity
{
    public partial class PasswordChangeLog : AuditableEntity
    {
        public int PasswordChangeLogId { get; set; }
        public int UserId { get; set; }
        public string? Password  { get; set; } = string.Empty;
        public string? PasswordHash { get; set; } = string.Empty;

        public virtual ApplicationUser User { get; set; } = null!;
    }
}
