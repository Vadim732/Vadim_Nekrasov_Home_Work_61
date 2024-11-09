using System.Threading.Tasks;
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
        string adminAvatar = "https://lastfm.freetls.fastly.net/i/u/ar0/3d4d85e22cd52ef84204fcc92c394f11.jpg";
        
        var roles = new [] { "admin", "user" };
        
        foreach (var role in roles)
        {
            if (await _roleManager.FindByNameAsync(role) == null)
                await _roleManager.CreateAsync(new IdentityRole<int>(role));
        }
        if (await _userManager.FindByNameAsync(adminEmail) == null)
        {
            User admin = new User { Email = adminEmail, UserName = adminUserName, Avatar = adminAvatar};
            IdentityResult result = await _userManager.CreateAsync(admin, adminPassword);
            if (result.Succeeded)
                await _userManager.AddToRoleAsync(admin, "admin");
        }
    }
}