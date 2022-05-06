using Microsoft.AspNetCore.Mvc;
using PlanSuite.Models.Temporary;
using PlanSuite.Services;

namespace PlanSuite.Controllers.Api
{
    [Route("api/[Controller]")]
    [ApiController]
    public class ProjectController : ControllerBase
    {
        private readonly ProjectService m_ProjectService;

        public ProjectController(ProjectService projectService)
        {
            m_ProjectService = projectService;
        }

        [HttpPost("movecard")]
        public IActionResult MoveCard([FromForm] MoveCardModel model)
        {
            Console.WriteLine($"MoveCard: {model.CardId} {model.ColumnId}");
            m_ProjectService.MoveCard(model);

            return Ok(new
            {
                Moved = true
            });

        }

        [HttpPost("editcarddesc")]
        public IActionResult EditCardDesc([FromBody] EditCardDescModel model)
        {
            Console.WriteLine($"EditCardDescModel: {model.CardId} {model.Description}");
            m_ProjectService.EditCardDesc(model);

            return Ok(new
            {
                Modified = true
            });

        }
    }
}
