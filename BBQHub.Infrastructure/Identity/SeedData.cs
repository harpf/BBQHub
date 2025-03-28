using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace BBQHub.Infrastructure.Identity
{
    public static class SeedData
    {
        public static async Task EnsureAdminUserAsync(IServiceProvider services)
        {
            var userManager = services.GetRequiredService<UserManager<IdentityUser>>();
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

            const string adminEmail = "admin@bbqhub.local";
            const string adminPassword = "BBQadmin123!";
            const string adminRole = "Admin";

            // Rolle anlegen, falls nicht vorhanden
            if (!await roleManager.RoleExistsAsync(adminRole))
            {
                await roleManager.CreateAsync(new IdentityRole(adminRole));
            }

            // Benutzer anlegen, falls nicht vorhanden
            var admin = await userManager.FindByEmailAsync(adminEmail);
            if (admin == null)
            {
                admin = new IdentityUser { UserName = adminEmail, Email = adminEmail, EmailConfirmed = true };
                await userManager.CreateAsync(admin, adminPassword);
                await userManager.AddToRoleAsync(admin, adminRole);
            }
            else
            {
                // sicherstellen, dass er die Rolle hat
                if (!await userManager.IsInRoleAsync(admin, adminRole))
                {
                    await userManager.AddToRoleAsync(admin, adminRole);
                }
            }
        }
    }
}
