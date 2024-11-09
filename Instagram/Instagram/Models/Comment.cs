using System.ComponentModel.DataAnnotations;

namespace Instagram.Models;

public class Comment
{
    public int Id { get; set; }
    [Required]
    public string Text { get; set; }
    public int UserId { get; set; }
    public User User { get; set; }

    public int PublicationId { get; set; }
    public Publication Publication { get; set; }

    public DateTime CreatedAt { get; set; }
}