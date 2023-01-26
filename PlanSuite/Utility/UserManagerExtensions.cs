using Microsoft.AspNetCore.Identity;
using PlanSuite.Models.Persistent;

namespace PlanSuite.Utility
{
    public static class UserManagerExtensions
    {
        public static async Task<ApplicationUser> FindByGuidAsync(this UserManager<ApplicationUser> userManager, Guid userId)
        {
            return await userManager.FindByIdAsync(userId.ToString());
        }
    }
}
