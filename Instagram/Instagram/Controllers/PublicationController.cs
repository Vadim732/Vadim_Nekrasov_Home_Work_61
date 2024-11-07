using Instagram.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

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
}