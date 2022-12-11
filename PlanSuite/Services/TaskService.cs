using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PlanSuite.Data;
using PlanSuite.Models.Persistent;

namespace PlanSuite.Services
{
    public class TaskService
    {
        private readonly ApplicationDbContext m_Database;

        public TaskService(ApplicationDbContext dbContext)
        {
            m_Database = dbContext;
        }

        public async Task<IActionResult> ArchiveCard(int taskId)
        {
            Console.WriteLine($"ArchiveCard {taskId}");
            var task = m_Database.Cards.Where(card => card.Id == taskId).FirstOrDefault();
            if(task == null)
            {
                Console.WriteLine($"ArchiveCard {taskId} null");
                return new NotFoundResult();
            }

            task.IsFinished = !task.IsFinished;
            Console.WriteLine($"Card marked IsFinished {taskId} = ${task.IsFinished}");
            await m_Database.SaveChangesAsync();
            return new OkResult();
        }
    }
}
