using Microsoft.AspNetCore.Mvc;
using PlanSuite.Data;

namespace PlanSuite.Services
{
    public class TaskService
    {
        private readonly ApplicationDbContext m_Database;
        private readonly ILogger<TaskService> m_Logger;

        public TaskService(ApplicationDbContext dbContext, ILogger<TaskService> logger)
        {
            m_Database = dbContext;
            m_Logger = logger;
        }

        public async Task<IActionResult> ArchiveCard(int taskId)
        {
            m_Logger.LogInformation($"ArchiveCard {taskId}");
            var task = m_Database.Cards.Where(card => card.Id == taskId).FirstOrDefault();
            if(task == null)
            {
                m_Logger.LogError($"ArchiveCard {taskId} null");
                return new NotFoundResult();
            }

            task.IsFinished = !task.IsFinished;
            m_Logger.LogInformation($"Card marked IsFinished {taskId} = ${task.IsFinished}");
            await m_Database.SaveChangesAsync();
            return new OkResult();
        }
    }
}
