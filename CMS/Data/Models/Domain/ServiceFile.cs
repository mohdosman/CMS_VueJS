using System;
using System.Collections.Generic;

namespace CMS.Data.Models.Domain;

public sealed class ServiceFile : AuditableEntity
{
    public int ServiceFileId { get; set; }

    public string FileName { get; set; } = null!;

    public byte[] FileText { get; set; } = null!;

    public string FileTextXml { get; set; } = null!;

    public string ProviderNPI { get; set; } = null!;

    public int ProviderId { get; set; }

    public DateTime FileCreationDate { get; set; }

    public int? ServiceCount { get; set; }

    public int? ErrorCount { get; set; }

    public int? PostedCount { get; set; }

    public bool IsProcessed { get; set; }

    public bool IsInProcess { get; set; }

    public ICollection<ServiceFileImport> ServiceFileImports { get; set; } = new List<ServiceFileImport>();

    public Provider Provider { get; set; } = null!;
}

