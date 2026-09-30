using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace CrisisManagement.Mvc.ViewModels;

public sealed class LoginViewModel
{
    [Required(ErrorMessage = "Username is required.")]
    [Display(Name = "User ID/Email")]
    public string UserName { get; set; } = "";

    [Required(ErrorMessage = "Password is required.")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = "";

    // Shown beside the form; filled by the controller on every render, never bound from the post.
    [BindNever]
    public LoginInfo Info { get; set; } = new();
}

public sealed class LoginInfo
{
    public List<NoticeItem> Notices { get; set; } = [];
    public List<SupportContact> Support { get; set; } = [];
    public SchemaLinks? Schema { get; set; }
    public bool EntraEnabled { get; set; }
}

public sealed record NoticeItem(DateTime CreatedOn, string Text);
public sealed record SupportContact(string Name, string? Phone, string? Email);
public sealed record SchemaLinks(string SchemaUrl, string SampleXmlUrl, string LastUpdated);
