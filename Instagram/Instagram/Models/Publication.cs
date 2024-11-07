using System.ComponentModel.DataAnnotations;

namespace Instagram.Models;

public class Publication
{
    public string Id { get; set; }
    public string? Image { get; set; }
    [Required]
    public string Description { get; set; }
    
    public int? NumberLikesId { get; set; }
    public Like? NumberLikes { get; set; }
}