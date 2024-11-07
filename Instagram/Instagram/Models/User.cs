using Microsoft.AspNetCore.Identity;

namespace Instagram.Models;

public class User : IdentityUser<int>
{
    public string Avatar { get; set; }
    public string? Name { get; set; }
    public string? Gender { get; set; }
    public string? AboutUser { get; set; }
    
    public int? PublicationId { get; set; }
    public Publication? Publication { get; set; }
}