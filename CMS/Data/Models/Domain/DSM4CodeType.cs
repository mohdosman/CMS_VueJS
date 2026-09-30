using System;
using System.Collections.Generic;

namespace CMS.Data.Models.Domain;

public sealed class DSM4CodeType
{
    public short DSM4CodeTypeId { get; set; }

    public string DSM4CodeTypeDescription { get; set; } = null!;

    public ICollection<DSM4Code> DSM4Codes { get; set; } = new List<DSM4Code>();
}

