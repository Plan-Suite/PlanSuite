using Microsoft.AspNetCore.Mvc;
using PlanSuite.Models.Temporary;
using PlanSuite.Services;

namespace PlanSuite.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProjController : ControllerBase
    {
        private readonly ProjectService m_ProjectService;

        public ProjController(ProjectService projectService)
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
    }
}
