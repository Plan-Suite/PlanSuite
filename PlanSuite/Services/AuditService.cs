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

        public async Task InsertLogAsync(AuditLogCategory logCat, ClaimsPrincipal user, AuditLogType logType, object targetId)
        {
            var appUser = await m_UserManager.GetUserAsync(user);
            await InsertLogAsync(logCat, appUser, logType, targetId);
        }

        public async Task InsertLogAsync(AuditLogCategory logCat, ApplicationUser user, AuditLogType logType, object targetId)
        {
            AuditLog auditLog = new AuditLog();
            auditLog.LogType = logType;
            auditLog.UserID = Guid.Parse(user.Id);
            auditLog.Timestamp = DateTime.Now;
            auditLog.LogCategory = logCat;
            auditLog.TargetID = targetId.ToString();

            await m_Database.AuditLogs.AddAsync(auditLog);
            await m_Database.SaveChangesAsync();

            string logMsg = $"[{auditLog.Timestamp}] {auditLog.LogCategory} #{auditLog.Id} #{auditLog.TargetID} was {auditLog.LogType} by {user.UserName}";
            Console.WriteLine(logMsg);
            DateTime now = DateTime.Now;
            await File.AppendAllTextAsync($"/var/log/plansuite/audit_{now.Day}-{now.Month}-{now.Year}.log", logMsg);
        }

        internal Task InsertLogAsync(AuditLogCategory organisation, ApplicationUser user, object makeAdmin, object id)
        {
            throw new NotImplementedException();
        }
    }
}
