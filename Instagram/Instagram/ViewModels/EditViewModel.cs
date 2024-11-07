using System.ComponentModel.DataAnnotations;

namespace Instagram.ViewModels;

public class EditViewModel
{
    [Required(ErrorMessage = "Invalid login!")]
    public string UserName { get; set; }

    [Required(ErrorMessage = "Email is required!")]
    [EmailAddress(ErrorMessage = "Invalid email address!")]
    public string Email { get; set; }
    [Required(ErrorMessage = "Invalid login!")]
    [Url]
    public string Avatar { get; set; }

    [Required(ErrorMessage = "Password is required!")]
    [MinLength(8)]
    [DataType(DataType.Password)]
    public string Password { get; set; }
    
    public string? Name { get; set; }
    public string? AboutUser { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Gender { get; set; }
}