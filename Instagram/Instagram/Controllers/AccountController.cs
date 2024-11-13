using Instagram.Models;
using Instagram.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SignInResult = Microsoft.AspNetCore.Identity.SignInResult;

namespace Instagram.Controllers;

public class AccountController : Controller
{
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;
    private readonly InstagramContext _context;

    public AccountController(UserManager<User> userManager, SignInManager<User> signInManager, InstagramContext context)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _context = context;
    }
    
    
    [HttpGet]
    public IActionResult Register()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (ModelState.IsValid)
        {
            User user = new User()
            {
                UserName = model.UserName,
                Email = model.Email,
                Avatar = model.Avatar,
                AboutUser = model.AboutUser,
                PhoneNumber = model.PhoneNumber,
                Gender = model.Gender,
                Name = model.Name
            };
            var result = await _userManager.CreateAsync(user, model.Password);
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, "user");
                await _signInManager.SignInAsync(user, false);
                return RedirectToAction("Index", "Publication");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }

        return View(model);
    }
    
    [HttpGet]
    public IActionResult Login()
    {
        return View();
    }
    
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (ModelState.IsValid)
        {
            User user = await _userManager.FindByEmailAsync(model.Identifier) ?? await _userManager.FindByNameAsync(model.Identifier);
        
            if (user != null)
            {
                SignInResult result = await _signInManager.PasswordSignInAsync(user, model.Password, model.RememberMe, false);
                if (result.Succeeded)
                {
                    if (!string.IsNullOrEmpty(model.ReturnUrl))
                    {
                        return Redirect(model.ReturnUrl);
                    }
                    return RedirectToAction("Index", "Publication");
                }
            }
            
            ModelState.AddModelError("", "Incorrect login or password!");
        }

        return View(model);
    }
    
    [HttpGet]
    [Authorize]
    public async Task<IActionResult> Edit()
    {
        User user = await _userManager.GetUserAsync(User);
        var model = new EditViewModel
        {
            UserName = user.UserName,
            Email = user.Email,
            Avatar = user.Avatar,
            Name = user.Name,
            AboutUser = user.AboutUser,
            PhoneNumber = user.PhoneNumber,
            Gender = user.Gender
        };
        return View(model);
    }
    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(EditViewModel model)
    {
        if (ModelState.IsValid)
        {
            User user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                user.UserName = model.UserName;
                user.Email = model.Email;
                user.Avatar = model.Avatar;
                user.Name = model.Name;
                user.AboutUser = model.AboutUser;
                user.PhoneNumber = model.PhoneNumber;
                user.Gender = model.Gender;

                var result = await _userManager.UpdateAsync(user);
                if (result.Succeeded)
                {
                    return RedirectToAction("Profile", "Publication");
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
        }
    
        return View(model);
    }

public async Task<IActionResult> Search(string query)
{
    if (string.IsNullOrWhiteSpace(query))
    {
        return View("SearchResults", new List<User>());
    }
    query = query.ToLower();
    var users = await _context.Users
        .Where(u => u.UserName.ToLower().Contains(query) ||
                    u.Email.ToLower().Contains(query) ||
                    u.Name.ToLower().Contains(query) ||
                    u.AboutUser.ToLower().Contains(query))
        .ToListAsync();

    return View("SearchResults", users);
}

    public IActionResult AccessDenied()
    {
        return View();
    }


    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction("Login");
    }
}