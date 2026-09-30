using CMS.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMS.Data.Models.Identity
{
    public sealed class Logon : AuditableEntity
    {
        public int LogonId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string SessionId { get; set; } = string.Empty;
        public DateTime LogOnDateTime { get; set; }
        public DateTime LogOffDateTime { get; set; }
        public int UserId { get; set; }

        public ApplicationUser User { get; set; } = null!;
    }
}
