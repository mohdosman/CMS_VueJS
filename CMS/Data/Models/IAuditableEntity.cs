using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMS.Data.Models
{
    public interface IAuditableEntity
    {
        int CreatedBy { get; set; }
        int UpdatedBy { get; set; }
        DateTime CreatedOn { get; set; }
        DateTime UpdatedOn { get; set; }
        byte[] Version { get; set; }
    }
}
