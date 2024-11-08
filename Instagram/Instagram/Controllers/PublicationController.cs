﻿using Instagram.Models;
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
        List<Publication> publications = await _context.Publications.ToListAsync();
        return View(publications);
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> Create()
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
            publication.CreatedAt = DateTime.Now;

            _context.Add(publication);
            _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        return View(publication);
    }
}