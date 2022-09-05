using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using PlanSuite.Data;
using PlanSuite.Enums;
using PlanSuite.Models.Persistent;

namespace PlanSuite.Services
{
    public class AuditService
    {
        private readonly ApplicationDbContext m_Database;
        private readonly UserManager<ApplicationUser> m_UserManager;

        public AuditService(ApplicationDbContext dbContext, UserManager<ApplicationUser> userManager)
        {
            m_Database = dbContext;
            m_UserManager = userManager;
        }

        public async Task InsertLogAsync(AuditLogType logType, ClaimsPrincipal user, string message)
        {
            AuditLog auditLog = new AuditLog();
            auditLog.LogType = logType;

            var appUser = await m_UserManager.GetUserAsync(user);
            auditLog.UserID = Guid.Parse(appUser.Id);
            auditLog.Timestamp = DateTime.Now;
            auditLog.Message = message;

            await m_Database.AuditLogs.AddAsync(auditLog);
            await m_Database.SaveChangesAsync();
            Console.WriteLine($"[{auditLog.Timestamp}] {auditLog.LogType} #{auditLog.Id} (target: {auditLog.TargetID}): {message}");
        }
    }
}
