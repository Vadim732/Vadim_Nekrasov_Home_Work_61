using Instagram.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Instagram.Controllers;

public class PublicationController : Controller
{
    private readonly InstagramContext _context;
    private readonly UserManager<User> _userManager;

    public PublicationController(InstagramContext context, UserManager<User> userManager)
    {
        _context = context;
        _userManager = userManager;
    }
    
    public async Task<IActionResult> Profile()
    {
        User user = await _userManager.GetUserAsync(User);
        if (user != null)
        {
            return View(user);
        }

        return RedirectToAction("Login", "Account");
    }
    
    public async Task<IActionResult> Index()
    {
        var publications = await _context.Publications
            .Include(p => p.User)
            .ToListAsync();

        return View(publications);
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

        var publication = await _context.Publications
            .Include(p => p.Likes)
            .FirstOrDefaultAsync(p => p.Id == publicationId);

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

}