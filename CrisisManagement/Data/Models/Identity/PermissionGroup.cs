using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace CrisisManagement.Data.Models.Identity
{
    public partial class PermissionGroup : AuditableEntity
    {
        public PermissionGroup()
        {
            Permissions = new HashSet<Permission>();
        }

        public int PermissionGroupId { get; set; }
        public string GroupName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        public ICollection<Permission> Permissions { get; set; }
    }
}
