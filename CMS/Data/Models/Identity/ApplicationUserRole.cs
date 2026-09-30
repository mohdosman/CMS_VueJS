using CMS.Data.Models;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMS.Data.Models.Identity
{
    public class ApplicationUserRole : IdentityUserRole<int>, IAuditableEntity
    {
        public int UserInRoleId { get; private set; }
        public int CreatedBy { get; set; }
        public int UpdatedBy { get; set; }
        public DateTime CreatedOn { get; set; } = DateTime.Now;
        public DateTime UpdatedOn { get; set; } = DateTime.Now;
        public byte[] Version { get; set; } = [];
    }
}
