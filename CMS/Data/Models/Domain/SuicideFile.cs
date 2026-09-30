using System;
using System.Collections.Generic;

namespace CMS.Data.Models.Domain;

public sealed class SuicideFile : AuditableEntity
{
    public int SuicideFileId { get; set; }

    public string FileName { get; set; } = null!;

    public byte[] FileText { get; set; } = null!;

    public int FileSize { get; set; }

    public int RecordCount { get; set; }

    public ICollection<SuicideFileImport> SuicideFileImports { get; set; } = new List<SuicideFileImport>();
}

