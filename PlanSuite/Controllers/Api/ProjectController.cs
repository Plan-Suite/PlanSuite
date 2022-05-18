using Microsoft.AspNetCore.Mvc;
using PlanSuite.Models.Temporary;
using PlanSuite.Services;
using System.Text.Json;

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
        public IActionResult MoveCard([FromBody] MoveCardModel model)
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

        [HttpPost("editcardname")]
        public IActionResult EditCardDesc([FromBody] EditCardNameModel model)
        {
            Console.WriteLine($"EditCardNameModel: {model.CardId} {model.Name}");
            m_ProjectService.EditCardName(model);

            return Ok(new
            {
                Modified = true
            });

        }

        [HttpGet("getcard")]
        public ActionResult<GetCardReturnJson> GetCardMarkdown(int cardId/*[FromBody] GetCardMarkdownModel model*/)
        {
            Console.WriteLine($"GetCardReturnJson: {cardId}");
            GetCardReturnJson json = m_ProjectService.GetCardMarkdown(cardId);
            return json;

        }
    }
}
