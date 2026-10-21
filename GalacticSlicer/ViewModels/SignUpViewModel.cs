using System.ComponentModel.DataAnnotations;

namespace GalacticSlicer.ViewModels;

public class SignUpViewModel
{
    [Required(ErrorMessage = "Please choose a username.")]
    [StringLength(30, MinimumLength = 3, ErrorMessage = "Username must be between 3 and 30 characters.")]
    [RegularExpression(@"^[a-zA-Z0-9_.-]+$", ErrorMessage = "Username can only contain letters, numbers, _ . and -")]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please choose a password.")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [DataType(DataType.Password)]
    [Display(Name = "Confirm password")]
    [Compare("Password", ErrorMessage = "The passwords do not match.")]
    public string ConfirmPassword { get; set; } = string.Empty;
}