using CrisisManagement.Data.Models;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrisisManagement.Data.Models.Identity
{
    public class ApplicationRole : IdentityRole<int>, IAuditableEntity
    {
        /// <summary>
        /// Initializes a new instance of <see cref="ApplicationRole"/>.
        /// </summary>
        public ApplicationRole()
        {
            Users = new HashSet<ApplicationUserRole>();
            Claims = new HashSet<IdentityRoleClaim<int>>();
        }

        /// <summary>
        /// Initializes a new instance of <see cref="ApplicationRole"/>.
        /// </summary>
        /// <param name="roleName">The role name.</param>
        public ApplicationRole(string roleName) : base(roleName)
        {
            Users = new HashSet<ApplicationUserRole>();
            Claims = new HashSet<IdentityRoleClaim<int>>();
        }

        /// <summary>
        /// Initializes a new instance of <see cref="ApplicationRole"/>.
        /// </summary>
        /// <param name="roleName">The role name.</param>
        /// <param name="description">Description of the role.</param>
        public ApplicationRole(string roleName, string description) : base(roleName)
        {
            Users = new HashSet<ApplicationUserRole>();
            Claims = new HashSet<IdentityRoleClaim<int>>();
        }



        public Guid RoleKey { get; set; } = Guid.NewGuid();
        public int CreatedBy { get; set; }
        public int UpdatedBy { get; set; }
        public DateTime CreatedOn { get; set; } = DateTime.Now;
        public DateTime UpdatedOn { get; set; } = DateTime.Now;
        public byte[] Version { get; set; } = [];

        /// <summary>
        /// Navigation property for the users in this role.
        /// </summary>
        public virtual ICollection<ApplicationUserRole> Users { get; set; }

        /// <summary>
        /// Navigation property for claims in this role.
        /// </summary>
        public virtual ICollection<IdentityRoleClaim<int>> Claims { get; set; }
    }
}
