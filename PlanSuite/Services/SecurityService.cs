using Microsoft.AspNetCore.Identity;
using PlanSuite.Data;
using PlanSuite.Enums;
using PlanSuite.Models.Persistent;
using System.Security.Claims;

namespace PlanSuite.Services
{
    public class SecurityService
    {
        private readonly ApplicationDbContext m_Database;
        private readonly ILogger<SecurityService> m_Logger;
        private readonly UserManager<ApplicationUser> m_UserManager;

        public SecurityService(ApplicationDbContext database, ILogger<SecurityService> logger, UserManager<ApplicationUser> userManager)
        {
            m_Database = database;
            m_Logger = logger;
            m_UserManager = userManager;
        }
        
        // Write security logs to the database
        public async Task WriteLogAsync(ApplicationUser user, LogAction action, string area, string description, string oldValue = null, string newValue = null)
        {
            m_Logger.LogInformation($"[SECURITY] [User: {user.FullName} Action: {action} Area: {area} Old: {oldValue} New: {newValue}]: {description}");

            var newLog = new SecurityLog
            {
                Date = DateTime.Now,
                Action = action,
                Area = area,
                Description = description,
                UserId = user.UserId,
                OldValue = oldValue,
                NewValue = newValue
            };
            await m_Database.SecurityLogs.AddAsync(newLog);
            await m_Database.SaveChangesAsync();
        }

        public async Task WriteLogAsync(ClaimsPrincipal claimsPrincipal, LogAction action, string area, string description, string oldValue = null, string newValue = null)
        {
            if(claimsPrincipal == null)
            {
                return;
            }

            var appUser = await m_UserManager.GetUserAsync(claimsPrincipal);
            await WriteLogAsync(appUser, action, area, description, oldValue, newValue);
        }
    }
}
