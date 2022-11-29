using Microsoft.AspNetCore.Mvc;
using PlanSuite.Services;

namespace PlanSuite.Controllers.Api
{
    [Route("api/[Controller]")]
    [ApiController]
    public class TaskController : ControllerBase
    {
        private readonly TaskService m_TaskService;

        public TaskController(TaskService taskService)
        {
            m_TaskService = taskService;
        }

        [HttpGet("ArchiveTask")]
        public async Task<IActionResult> ArchiveTask([FromQuery] int taskId)
        {
            Console.WriteLine($"ArchiveCard: {taskId}");
            return await m_TaskService.ArchiveCard(taskId);
        }
    }
}
