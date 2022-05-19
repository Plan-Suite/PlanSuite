using Microsoft.AspNetCore.Identity;
using PlanSuite.Models.Persistent;
using System.Security.Cryptography;

namespace PlanSuite.Utility
{
    public static class PasswordReset
    {
        public static async Task<bool> SendPasswordReset(ApplicationUser user, UserManager<ApplicationUser> userManager)
        {
            var code = await userManager.GeneratePasswordResetTokenAsync(user);
            var link = $"http://plan-suite.com/api/Account/ResetPassword?code={code}";
            Console.WriteLine($"Generated password reset link for #{user.Email}: {link}");
            return true;
        }

        public static string GenerateRandomString()
        {
            using Aes crypto = Aes.Create();
            crypto.GenerateKey();
            return Convert.ToBase64String(crypto.Key);
        }
    }
}
