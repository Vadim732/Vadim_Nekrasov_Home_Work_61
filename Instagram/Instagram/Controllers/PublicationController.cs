using Instagram.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace Instagram.Controllers;

public class PublicationController : Controller
{
    private readonly InstagramContext _context;
    private readonly UserManager<User> _userManager;
    private readonly ILogger<PublicationController> _logger;

    public PublicationController(InstagramContext context, UserManager<User> userManager, ILogger<PublicationController> logger)
    {
        _context = context;
        _userManager = userManager;
        _logger = logger;
    }
    [Authorize]
    public async Task<IActionResult> Profile(int? userId)
    {
        var currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
        if (userId == null)
        {
            userId = currentUserId;
        }

        var user = await _context.Users
            .Include(u => u.Publications)
            .ThenInclude(p => p.Comments)
            .ThenInclude(c => c.User)
            .FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null)
        {
            return NotFound($"Пользователь не найден.");
        }

        if (user != null)
        {
            ViewBag.Publications = user.Publications;
            ViewBag.currentUserId = currentUserId;
            var isFollowing = await _context.Follows.AnyAsync(f => f.FollowerId == currentUserId && f.FollowingId == user.Id);
            ViewBag.IsFollowing = isFollowing;
            return View(user);
        }

        return NotFound();
    }


    public async Task<IActionResult> Index()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return RedirectToAction("Login", "Account");
        }

        ViewBag.CurrentUserId = user.Id;

        List<Publication> publications;
        try
        {
            publications = await _context.Publications
                .Include(p => p.User)
                .Include(p => p.Comments)
                    .ThenInclude(c => c.User)
                .ToListAsync();

            if (publications == null || !publications.Any())
            {
                ViewBag.Message = "No publications found.";
                return View(new List<Publication>()); 
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while retrieving publications.");
            ViewBag.ErrorMessage = "An error occurred while loading publications.";
            return View(new List<Publication>()); 
        }

        return View(publications);
    }


    [HttpGet]
    public async Task<IActionResult> Details(int publicationId)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user != null)
        {
            ViewBag.CurrentUserId = user.Id;
        }
        var publication = await _context.Publications.Include(p => p.User).Include(p => p.Comments).ThenInclude(c => c.User).FirstOrDefaultAsync(p => p.Id == publicationId);
        
        if (publication != null)
        {
            return View(publication);
        }
        
        return NotFound("Publication not found.");
    }


    [HttpGet]
    [Authorize]
    public IActionResult Create()
    {
        return View();
    }
    
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Create(Publication publication)
    {
        if (ModelState.IsValid)
        {
            var creator = await _userManager.GetUserAsync(User);
            publication.UserId = creator.Id;
            publication.CreatedAt = DateTime.UtcNow;

            _context.Add(publication);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        return View(publication);
    }
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> ToggleLike(int publicationId)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return RedirectToAction("Login", "Account");

        var publication = await _context.Publications.Include(p => p.Likes).FirstOrDefaultAsync(p => p.Id == publicationId);

        if (publication == null) return NotFound();

        var existingLike = publication.Likes.FirstOrDefault(l => l.UserId == user.Id);
        if (existingLike != null)
        {
            publication.Likes.Remove(existingLike);
            publication.LikeCount--;
        }
        else
        {
            publication.Likes.Add(new Like { UserId = user.Id, PostId = publicationId , CreatedAt = DateTime.UtcNow});
            publication.LikeCount++;
        }

        await _context.SaveChangesAsync();
        return RedirectToAction("Index");
    }
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Follow(int followingId)
    {
        var follower = await _userManager.GetUserAsync(User);
        if (follower == null || follower.Id == followingId)
            return BadRequest("Invalid follow request.");

        var following = await _context.Users.Include(u => u.Followers).FirstOrDefaultAsync(u => u.Id == followingId);
        if (following == null)
            return NotFound();
        if (await _context.Follows.AnyAsync(f => f.FollowerId == follower.Id && f.FollowingId == followingId))
            return BadRequest("You are already following this user.");

        var follow = new Follow
        {
            FollowerId = follower.Id,
            FollowingId = followingId
        };

        _context.Follows.Add(follow);
        follower.FollowingCount++;
        following.FollowersCount++;

        await _context.SaveChangesAsync();

        return RedirectToAction("Profile", new { userId = followingId });
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Unfollow(int followingId)
    {
        var follower = await _userManager.GetUserAsync(User);
        if (follower == null || follower.Id == followingId)
            return BadRequest("Invalid unfollow request.");

        var following = await _context.Users.Include(u => u.Followers)
            .FirstOrDefaultAsync(u => u.Id == followingId);

        if (following == null)
            return NotFound();
        var follow = await _context.Follows.FirstOrDefaultAsync(f => f.FollowerId == follower.Id && f.FollowingId == followingId);

        if (follow == null)
            return BadRequest("You are not following this user.");

        _context.Follows.Remove(follow);
        follower.FollowingCount--;
        following.FollowersCount--;

        await _context.SaveChangesAsync();

        return RedirectToAction("Profile", new { userId = followingId });
    }
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> AddComment(int publicationId, string text)
    {
        if (string.IsNullOrEmpty(text))
            return BadRequest("Comment text cannot be empty.");

        var user = await _userManager.GetUserAsync(User);
        if (user == null) return RedirectToAction("Login", "Account");
        var publication = await _context.Publications
            .FirstOrDefaultAsync(p => p.Id == publicationId);

        if (publication == null)
            return NotFound("Publication not found.");

        var comment = new Comment
        {
            Text = text,
            UserId = user.Id,
            PublicationId = publicationId,
            CreatedAt = DateTime.UtcNow
        };

        publication.Comments.Add(comment);
        publication.CommentCount++;

        await _context.SaveChangesAsync();
        return RedirectToAction("Index");
    }
    [Authorize]
    public async Task<IActionResult> FollowedPublications()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user != null)
        {
            ViewBag.CurrentUserId = user.Id;
        }
        if (user == null) return RedirectToAction("Login", "Account");
        var followedUserIds = await _context.Follows
            .Where(f => f.FollowerId == user.Id)
            .Select(f => f.FollowingId)
            .ToListAsync();
        var followedPublications = await _context.Publications
            .Where(p => followedUserIds.Contains(p.UserId))
            .Include(p => p.User)
            .Include(p => p.Comments)
            .ThenInclude(c => c.User)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();

        return View(followedPublications);
    }
    
    [HttpGet]
    [Authorize]
    public async Task<IActionResult> EditPost(int publicationId)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user != null)
        {
            var publication = await _context.Publications.FirstOrDefaultAsync(p => p.Id == publicationId && p.UserId == user.Id);
            if (publication != null)
            {
                return View(publication);
            }
            
            return NotFound();
        }
        
        return RedirectToAction("Login", "Account");
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> EditPost(Publication postEdit)
    {
        if (ModelState.IsValid)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                var publication = await _context.Publications.FirstOrDefaultAsync(p => p.Id == postEdit.Id && p.UserId == user.Id);
                if (publication != null)
                {
                    publication.Description = postEdit.Description;
                    publication.Image = postEdit.Image;

                    await _context.SaveChangesAsync();
                    return RedirectToAction("Details", new { publicationId = postEdit.Id });
                }
                
                return NotFound();
            }

            return RedirectToAction("Login", "Account");
        }

        return View(postEdit);
    }
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> DeleteComment(int commentId)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) 
            return RedirectToAction("Login", "Account");

        var comment = await _context.Comments
            .Include(c => c.Publication)
            .FirstOrDefaultAsync(c => c.Id == commentId && c.UserId == user.Id);

        if (comment == null) 
            return BadRequest("Comment not found or you do not have permission to delete this comment.");

        _context.Comments.Remove(comment);
        comment.Publication.CommentCount--;

        await _context.SaveChangesAsync();
        return RedirectToAction("Details", new { publicationId = comment.PublicationId });
    }
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> EditComment(int commentId, string newText)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return RedirectToAction("Login", "Account");
        var comment = await _context.Comments.FindAsync(commentId);
        if (comment == null) return NotFound();
        if (comment.UserId != user.Id)
            return Forbid("You are not authorized to edit this comment.");
        comment.Text = newText;
        await _context.SaveChangesAsync();

        return RedirectToAction("Index");
    }
    
    [HttpGet]
    [Authorize]
    public async Task<IActionResult> DeletePost(int publicationId)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user != null)
        {
            var publication = await _context.Publications.FirstOrDefaultAsync(p => p.Id == publicationId && p.UserId == user.Id);

            if (publication != null)
            {
                return View(publication);
            }
            
            return NotFound();
        }
        
        return RedirectToAction("Login", "Account");
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> DeletePostConfirmed(int publicationId)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user != null)
        {
            var publication = await _context.Publications.FirstOrDefaultAsync(p => p.Id == publicationId && p.UserId == user.Id);
            if (publication != null)
            {
                _context.Publications.Remove(publication);
                await _context.SaveChangesAsync();

                return RedirectToAction("Index");
            }

            return NotFound();
        }
        
        return RedirectToAction("Login", "Account");
    }
}