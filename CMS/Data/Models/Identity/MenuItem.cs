using CMS.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMS.Data.Models.Identity
{
    public partial class MenuItem : AuditableEntity
    {
        public MenuItem()
        {
            Permissions = new HashSet<Permission>();
        }

        public int MenuItemId { get; set; }
        public string Icon { get; set; } = string.Empty;
        public string MenuItemName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public string DetailUrl { get; set; } = string.Empty;
        public string TemplateUrl { get; set; } = string.Empty;
        public string DetailTemplateUrl { get; set; } = string.Empty;
        public string ApiUrl { get; set; } = string.Empty;
        public int? ParentMenuItemId { get; set; }
        public byte DisplaySequence { get; set; }
        public bool IsAlwaysEnabled { get; set; }
        public bool IsEnabled { get; set; } = true;
        public string Comment { get; set; } = string.Empty;

        public virtual ICollection<Permission> Permissions { get; set; }
    }
}
