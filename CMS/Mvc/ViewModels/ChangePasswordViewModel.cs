using System.ComponentModel.DataAnnotations;

namespace CMS.Mvc.ViewModels;

public sealed class ChangePasswordViewModel
{
    [Required(ErrorMessage = "Current password is required.")]
    [DataType(DataType.Password)]
    [Display(Name = "Current password")]
    public string CurrentPassword { get; set; } = "";

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
