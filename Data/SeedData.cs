using Microsoft.AspNetCore.Identity;
using PetShow.Models;

namespace PetShow.Data;

public static class SeedData
{
    public static async Task InitializeAsync(IServiceProvider services)
    {
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

        // ── Создание роли ──────────────────────────────────
        string[] roles = { "Admin", "Organizer", "User" };
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                var result = await roleManager.CreateAsync(new IdentityRole(role));
                if (!result.Succeeded)
                    throw new Exception($"Не удалось создать роль '{role}': " +
                        string.Join(", ", result.Errors.Select(e => e.Description)));
            }
        }

        // ── Данные админского аккаунта ──────────────────────────────────────
        const string adminEmail    = "admin@petshow.com";
        const string adminPassword = "Admin1234!";

        var existingAdmin = await userManager.FindByEmailAsync(adminEmail);
        if (existingAdmin == null)
        {
            var admin = new ApplicationUser
            {
                UserName         = adminEmail,
                Email            = adminEmail,
                FullName         = "Администратор сайта",
                EmailConfirmed   = true,
                CreatedAt        = DateTime.UtcNow
            };

            var result = await userManager.CreateAsync(admin, adminPassword);
            if (result.Succeeded)
                await userManager.AddToRoleAsync(admin, "Admin");
        }
        else
        {
                // Убеждаемся, что существующая учетная запись администратора имеет роль «Администратор».
            if (!await userManager.IsInRoleAsync(existingAdmin, "Admin"))
                await userManager.AddToRoleAsync(existingAdmin, "Admin");
        }
    }
}