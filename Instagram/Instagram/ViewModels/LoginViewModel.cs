using System.ComponentModel.DataAnnotations;

namespace Instagram.ViewModels;

public class LoginViewModel
{
    [Required(ErrorMessage = "Invalid login or email!")]
    public string Identifier { get; set; }
    [Required(ErrorMessage = "Password is required!")]
    [DataType(DataType.Password)]
    [MinLength(8)]
    public string Password { get; set; }
    public bool RememberMe { get; set; }
    public string? ReturnUrl { get; set; }
}