using Microsoft.AspNetCore.Identity;

namespace Instagram.Models;

public class User : IdentityUser<int>
{
    public string Avatar { get; set; }
    public string? Name { get; set; }
    public string? Gender { get; set; }
    public string? AboutUser { get; set; }
    
    public int PublicationCount { get; set; } = 0;
    public int FollowersCount { get; set; } = 0;
    public int FollowingCount { get; set; } = 0;

    public ICollection<Publication> Publications { get; set; } = new List<Publication>();
    public ICollection<Follow> Followers { get; set; } = new List<Follow>();
    public ICollection<Follow> Following { get; set; } = new List<Follow>();
    public ICollection<Like> Likes { get; set; } = new List<Like>();
    public ICollection<Comment> Comments { get; set; } = new List<Comment>();
}