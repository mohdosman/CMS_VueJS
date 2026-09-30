using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMS.Data.Models.Identity
{
    public partial class Permission : AuditableEntity
    {
        public int PermissionId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int PermissionGroupNameId { get; set; }
        public int MenuItemId { get; set; }

        public virtual MenuItem MenuItem { get; set; } = null!;
        public virtual PermissionGroup PermissionGroupName { get; set; } = null!;
    }
}
