using System;
using System.Collections.Generic;

namespace CrisisManagement.Data.Models.Domain;

public sealed class SuicideFileImport : AuditableEntity
{
    public int SuicideFileImportId { get; set; }

    public int SuicideFileId { get; set; }

    public string? DDeathStateCountry { get; set; }

    public string? DNameFirst { get; set; }

    public string? DNameMiddle { get; set; }

    public string? DNameLast { get; set; }

    public string? DSex { get; set; }

    public string? DDODMo { get; set; }

    public string? DDODDay { get; set; }

    public string? DDODYr { get; set; }

    public string? DDOBMo { get; set; }

    public string? DDOBDay { get; set; }

    public string? DDOBYr { get; set; }

    public string? DSSN { get; set; }

    public string? DResStateCountry { get; set; }

    public string? DResCounty { get; set; }

    public string? DUSArmedForces { get; set; }

    public string? DDeathManner { get; set; }

    public string? Provider { get; set; }

    public SuicideFile SuicideFile { get; set; } = null!;
}

