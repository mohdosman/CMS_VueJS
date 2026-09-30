using CrisisManagement.Data.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrisisManagement.Data.Models
{
    public class AuditableEntity : IAuditableEntity
    {
        [Required]
        public int CreatedBy { get; set; }

        [Required]
        public int UpdatedBy { get; set; }

        [Required]
        public DateTime UpdatedOn { get; set; } = DateTime.Now;

        [Required]
        public DateTime CreatedOn { get; set; } = DateTime.Now;

        public byte[] Version { get; set; } = [];
    }
}
