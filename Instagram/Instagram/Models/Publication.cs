using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Instagram.Models;

public class Publication
{
    public int Id { get; set; }
    public string? Image { get; set; }
    [Required]
    public string Description { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public int LikeCount { get; set; } = 0;
    public int CommentCount { get; set; } = 0;
    public int UserId { get; set; }
    public User? User { get; set; }
    public ICollection<Like>? Likes { get; set; } = new List<Like>();
    public ICollection<Comment>? Comments { get; set; } = new List<Comment>();
}