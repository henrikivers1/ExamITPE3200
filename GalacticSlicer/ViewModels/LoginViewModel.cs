using System.ComponentModel.DataAnnotations;

namespace GalacticSlicer.ViewModels;

public class LoginViewModel
{
    [Required(ErrorMessage = "Please enter your username.")]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please enter your password.")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [Display(Name = "Remember me")]
    public bool RememberMe { get; set; }

    // The page the user tried to open before being sent to login
    public string? ReturnUrl { get; set; }
}