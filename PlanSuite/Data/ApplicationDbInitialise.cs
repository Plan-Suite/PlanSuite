using Microsoft.AspNetCore.Identity;
using PlanSuite.Models.Persistent;
using PlanSuite.Utility;

namespace PlanSuite.Data
{
    public static class ApplicationDbInitialise
    {
        public static async Task Initialize(IServiceProvider serviceProvider)
        {
            var context = serviceProvider
                .GetRequiredService<ApplicationDbContext>();
            context.Database.EnsureCreated();
            var roleManager = serviceProvider
                .GetRequiredService<RoleManager<IdentityRole>>();

            var userManager = serviceProvider
                .GetRequiredService<UserManager<ApplicationUser>>();

            IdentityResult result;
            bool roleExists = await roleManager.RoleExistsAsync(Constants.AdminRole);
            if (!roleExists)
            {
                var role = new IdentityRole();
                role.Name = Constants.AdminRole;
                await roleManager.CreateAsync(role);
            }

            roleExists = await roleManager.RoleExistsAsync(Constants.DevRole);
            if (!roleExists)
            {
                var role = new IdentityRole();
                role.Name = Constants.DevRole;
                await roleManager.CreateAsync(role);
            }

            roleExists = await roleManager.RoleExistsAsync(Constants.SalesRole);
            if (!roleExists)
            {
                var role = new IdentityRole();
                role.Name = Constants.SalesRole;
                await roleManager.CreateAsync(role);
            }

            roleExists = await roleManager.RoleExistsAsync(Constants.SupportRole);
            if (!roleExists)
            {
                var role = new IdentityRole();
                role.Name = Constants.SupportRole;
                await roleManager.CreateAsync(role);
            }

            // Create superuser account
            string rootPassword = PasswordReset.GenerateRandomString();
            var root = await userManager.FindByNameAsync("root");
            if (root == null)
            {
                root = new ApplicationUser();
                root.UserName = "root";
                root.Email = "noreply@plan-suite.com";

                IdentityResult chkUser = await userManager.CreateAsync(root, rootPassword);

                //Add default User to Role Admin    
                if (chkUser.Succeeded)
                {
                    await userManager.AddToRoleAsync(root, Constants.AdminRole);
                    var token = await userManager.GenerateEmailConfirmationTokenAsync(root);
                    await userManager.ConfirmEmailAsync(root, token);
                }
            }
            else
            {
                var token = await userManager.GeneratePasswordResetTokenAsync(root);
                await userManager.ResetPasswordAsync(root, token, rootPassword);
            }
            Console.WriteLine($"Root in {rootPassword}");
        }
    }
}