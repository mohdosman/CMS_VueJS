using System.ComponentModel.DataAnnotations;

namespace CMS.Mvc.ViewModels;

public sealed class ForgotPasswordViewModel
{
    [Required(ErrorMessage = "User ID/Email is required.")]
    [Display(Name = "User ID/Email")]
    public string UserNameOrEmail { get; set; } = "";
}

public sealed class ResetPasswordViewModel
{
    // Both come from the emailed link and travel back in hidden fields.
    public int UserId { get; set; }
    public string Token { get; set; } = "";

    [Required(ErrorMessage = "New password is required.")]
    [DataType(DataType.Password)]
    [Display(Name = "New password")]
    public string Password { get; set; } = "";

    [Required(ErrorMessage = "Confirm new password is required.")]
    [DataType(DataType.Password)]
    [Display(Name = "Confirm new password")]
    [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
    public string ConfirmPassword { get; set; } = "";
}
