namespace Instagram.Models;

public class Like
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public User User { get; set; }
    public int PostId { get; set; }
    public Publication Publication { get; set; }

    public DateTime CreatedAt { get; set; }
}