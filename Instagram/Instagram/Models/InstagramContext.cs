using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Instagram.Models;

public class InstagramContext : IdentityDbContext<User, IdentityRole<int>, int>
{
    public DbSet<User> Users { get; set; }
    public DbSet<Publication> Publications { get; set; }
    public DbSet<Like> Likes { get; set; }
    public DbSet<Comment> Comments { get; set; }
    public DbSet<Follow> Follows { get; set; }
    
    public InstagramContext(DbContextOptions<InstagramContext> options) : base(options) {}
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<Follow>()
            .HasIndex(f => new { f.FollowerId, f.FollowingId })
            .IsUnique();
        modelBuilder.Entity<Like>()
            .HasIndex(l => new { l.UserId, l.PostId })
            .IsUnique();
        modelBuilder.Entity<Follow>()
            .HasOne(f => f.Follower)
            .WithMany(u => u.Following)
            .HasForeignKey(f => f.FollowerId)
            .OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<Follow>()
            .HasOne(f => f.Following)
            .WithMany(u => u.Followers)
            .HasForeignKey(f => f.FollowingId)
            .OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<Publication>()
            .HasOne(p => p.User)
            .WithMany(u => u.Publications)
            .HasForeignKey(p => p.UserId);
        modelBuilder.Entity<Like>()
            .HasOne(l => l.Publication)
            .WithMany(p => p.Likes)
            .HasForeignKey(l => l.PostId);
        modelBuilder.Entity<Comment>()
            .HasOne(c => c.Publication)
            .WithMany(p => p.Comments)
            .HasForeignKey(c => c.PublicationId);
        modelBuilder.Entity<Comment>()
            .HasOne(c => c.User)
            .WithMany(u => u.Comments)
            .HasForeignKey(c => c.UserId);
        modelBuilder.Entity<Like>()
            .HasOne(l => l.User)
            .WithMany(u => u.Likes)
            .HasForeignKey(l => l.UserId);
    }
}