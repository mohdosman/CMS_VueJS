using CrisisManagement.Data.Models;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrisisManagement.Data.Models.Identity
{
    using CrisisManagement.Data.Models.Domain;
    using Microsoft.AspNetCore.Identity;

    public sealed class ApplicationUser : IdentityUser<int>, IAuditableEntity
    {
        // SQL Server's legacy "datetime" column type can't store DateTime.MinValue
        // (0001-01-01); this is its actual minimum, used as the "never happened yet"
        // sentinel for a user who hasn't logged in / been locked out.
        private static readonly DateTime SqlDateTimeMinValue = new(1753, 1, 1);

        #region Constructor
        public ApplicationUser()
        {
            EmailConfirmed = true;
        }
        #endregion

        public string FullName
        {
            get
            {
                var full = $"{FirstName} {LastName}".Trim();
                return string.IsNullOrWhiteSpace(full)
                    ? UserName ?? Email ?? ""
                    : full;
            }
        }

        #region Properties

        public Guid UserKey { get; set; } = Guid.NewGuid();
        public string? Comment { get; set; } = string.Empty; 
        public string FirstName { get; set; } = string.Empty;                 
        public string LastName { get; set; } = string.Empty;       
        public bool IsADAccount { get; set; } = false;
        public bool IsActive { get; set; } = false;                            
        public DateTime LastPasswordChangedDate { get; set; } = DateTime.Now;   

        // Identity uses LockoutEnd. Legacy also has IsLockedOut + LastLockoutDate.
        public bool IsLockedOut => LockoutEnabled && LockoutEnd >= DateTimeOffset.Now;

        // Map to RBS_User.IsLockedOut (legacy lockout flag, checked only at login - see HybridUserManager)
        public bool LegacyIsLockedOut { get; set; }

        // Map to RBS_User.Password (encrypted legacy password)
        public string LegacyPassword { get; set; } = string.Empty;             // RBS_User.Password

        public bool IsTemporaryPassword { get; set; }
        public DateTime LastLoginDate { get; set; } = SqlDateTimeMinValue;
        public DateTime LastLockoutDate { get; set; } = SqlDateTimeMinValue;
        public int FailedPasswordAttemptCount { get; set; }                    
        
        // These are set by your auditing in SaveChanges
        public int CreatedBy { get; set; }                                     
        public int UpdatedBy { get; set; }                                     
        public DateTime CreatedOn { get; set; }                                
        public DateTime UpdatedOn { get; set; }                                

        public byte[] Version { get; set; } = [];             

        // Navigation
        public ICollection<ApplicationUserRole> Roles { get; } = new HashSet<ApplicationUserRole>();
        public ICollection<IdentityUserClaim<int>> Claims { get; } = new HashSet<IdentityUserClaim<int>>();
        public ICollection<Logon> Logons { get; } = new HashSet<Logon>();
        public ICollection<PasswordChangeLog> PasswordChangeLogs { get; } = new HashSet<PasswordChangeLog>();
        public ICollection<FacilityUser> FacilityUsers { get; set; } = new List<FacilityUser>();
        public ICollection<ProviderUser> ProviderUsers { get; set; } = new List<ProviderUser>();

        #endregion
    }
}
