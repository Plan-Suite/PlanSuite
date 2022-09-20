using System.Runtime.InteropServices;
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

            string auditLogMsg = await AuditLogToHumanReadable(auditLog);
            string logMsg = $"[{auditLog.Timestamp}] (Category: {auditLog.LogCategory}, Id: #{auditLog.Id}, TargetId: #{auditLog.TargetID}, Action: {auditLog.LogType}, User: {user.UserName}): {auditLogMsg}";
            Console.WriteLine(logMsg);
            DateTime now = DateTime.Now;

            // We're not gonna do this on windows since we don't need to save logs when deving
#if !DEBUG
            await File.AppendAllTextAsync($"/var/log/plansuite/audit_{now.Day}-{now.Month}-{now.Year}.log", logMsg);
#endif
        }

        public async Task<string> AuditLogToHumanReadable(AuditLog auditLog)
        {
            switch (auditLog.LogCategory)
            {
                case AuditLogCategory.Project:
                    return await LogCategoryProjectHandler(auditLog);
                case AuditLogCategory.Milestone:
                    return await LogCategoryMilestoneHandler(auditLog);
            }
            return auditLog.LogType.ToString();
        }

        private async Task<string> LogCategoryProjectHandler(AuditLog auditLog)
        {
            // Target user for the audit log
            ApplicationUser targetUser = await m_UserManager.FindByIdAsync(auditLog.TargetID);

            string message = auditLog.LogType.ToString();
            switch (auditLog.LogType)
            {
                case AuditLogType.Created:
                    message = $"Created project.";
                    break;
                case AuditLogType.AddedMember:
                    message = $"Added user {targetUser.UserName} to project.";
                    break;
                case AuditLogType.Left:
                    message = $"Left the project.";
                    break;
                case AuditLogType.RemovedMember:
                    message = $"Removed user {targetUser.UserName} to project.";
                    break;
            }

            return message;
        }

        private async Task<string> LogCategoryMilestoneHandler(AuditLog auditLog)
        {
            // User who did the logged action
            ApplicationUser loggedUser = await m_UserManager.FindByIdAsync(auditLog.UserID.ToString());

            string message = string.Empty;
            switch (auditLog.LogType)
            {
                case AuditLogType.Modified:
                    message = $"Added user {loggedUser.UserName} to project.";
                    break;
                case AuditLogType.Closed:
                    break;
                case AuditLogType.AddedMilestone:
                    break;
            }

            return message;
        }
    }
}
