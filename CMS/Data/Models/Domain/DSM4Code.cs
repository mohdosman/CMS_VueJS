using System;
using System.Collections.Generic;

namespace CMS.Data.Models.Domain;

public sealed class DSM4Code
{
    public int DSM4CodeId { get; set; }

    public string? dsm_table { get; set; }

    public string? DSM4_CODE { get; set; }

    public string? dsm_description { get; set; }

    public string? ICD9_CODE { get; set; }

    public string? axis_code { get; set; }

    public string? axis_value { get; set; }

    public decimal? dss_facility_id { get; set; }

    public short? DSM4CodeTypeId { get; set; }

    public DSM4CodeType? DSM4CodeType { get; set; }
}

