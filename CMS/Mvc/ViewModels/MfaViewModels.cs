using System.ComponentModel.DataAnnotations;

namespace CMS.Mvc.ViewModels;

// The code asked for after the password when the account requires MFA and is enrolled.
public sealed class VerifyMfaViewModel
{
    [Required(ErrorMessage = "Authenticator code is required.")]
    [Display(Name = "Authenticator code")]
    public string Code { get; set; } = "";
}

public sealed class SetupMfaViewModel
{
    [Display(Name = "Authenticator code")]
    public string Code { get; set; } = "";

    // Filled by the controller on every render, never bound from the post.
    public bool Enrolled { get; set; }
    public bool Required { get; set; }
    public string SharedKey { get; set; } = "";
    public string QrCodeDataUri { get; set; } = "";
}
