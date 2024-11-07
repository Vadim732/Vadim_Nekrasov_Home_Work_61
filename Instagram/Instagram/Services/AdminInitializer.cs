using Instagram.Models;
using Microsoft.AspNetCore.Identity;

namespace Instagram.Services;

public class AdminInitializer
{
    public static async Task SeedRolesAndAdmin(RoleManager<IdentityRole<int>> _roleManager, UserManager<User> _userManager)
    {
        string adminEmail = "admin@admin.admin";
        string adminUserName = "AdminIvanov";
        string adminPassword = "zXcAdmin123$QwEaSd";
        
        var roles = new [] { "admin", "user" };
        
        foreach (var role in roles)
        {
            if (await _roleManager.FindByNameAsync(role) == null)
                await _roleManager.CreateAsync(new IdentityRole<int>(role));
        }
        if (await _userManager.FindByNameAsync(adminEmail) == null)
        {
            User admin = new User { Email = adminEmail, UserName = adminUserName };
            IdentityResult result = await _userManager.CreateAsync(admin, adminPassword);
            if (result.Succeeded)
                await _userManager.AddToRoleAsync(admin, "admin");
        }
    }
}