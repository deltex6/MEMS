using Microsoft.AspNetCore.Identity;

namespace MedicalEquipmentManagementSystem.Data;

/// <summary>
/// Klasa odpowiedzialna za seeding ról i domyślnych kont.
/// </summary>
public static class RoleSeeder
{
    /// <summary>
    /// Inicjalizuje podstawowe role i konta w systemie.
    /// </summary>
    /// <param name="serviceProvider">Dostawca usług do rozwiązania zależności.</param>
    public static async Task InitializeAsync(IServiceProvider serviceProvider)
    {
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();

        // Tworzenie ról, jeśli nie istnieją
        string[] roleNames = { UserRoles.Administrator, UserRoles.Technician, UserRoles.Staff };

        foreach (var roleName in roleNames)
        {
            var roleExists = await roleManager.RoleExistsAsync(roleName);
            if (!roleExists)
            {
                await roleManager.CreateAsync(new IdentityRole(roleName));
            }
        }

        // Tworzenie domyślnego konta Administratora
        var adminEmail = "admin@mems.local";
        var adminUser = await userManager.FindByEmailAsync(adminEmail);

        if (adminUser is null)
        {
            adminUser = new IdentityUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true
            };

            // Hasło dla celów deweloperskich/początkowych
            var createPowerUser = await userManager.CreateAsync(adminUser, "Admin123!");
            if (createPowerUser.Succeeded)
            {
                // Przypisanie użytkownika do roli Administrator
                await userManager.AddToRoleAsync(adminUser, UserRoles.Administrator);
            }
        }

        // Przypisanie konta ab@ab.pl do roli Administratora, o ile już istnieje
        var customAdminEmail = "ab@ab.pl";
        var customAdminUser = await userManager.FindByEmailAsync(customAdminEmail);

        if (customAdminUser is not null)
        {
            var isInRole = await userManager.IsInRoleAsync(customAdminUser, UserRoles.Administrator);
            if (!isInRole)
            {
                await userManager.AddToRoleAsync(customAdminUser, UserRoles.Administrator);
            }
        }
    }
}
