using System;
using System.Collections.Generic;

namespace CMS.Data.Models.Domain;

public sealed class EndUserAgreement : AuditableEntity
{
    public int EndUserAgreementId { get; set; }

    public bool HasAccepted { get; set; }

    public string? SessionId { get; set; }

}

