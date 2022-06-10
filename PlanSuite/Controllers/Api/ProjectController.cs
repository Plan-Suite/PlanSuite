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

        [HttpPost("EditCard")]
        public IActionResult EditCardDueDate([FromBody] EditCardDueDateModel model)
        {
            Console.WriteLine($"EditCardDueDateModel: {model.CardId} {model.Timestamp} {model.Priority} {model.AssigneeId}");
            m_ProjectService.EditCardDueDate(model);

            return Ok(new
            {
                Modified = true
            });

        }

        [HttpPost("leaveproject")]
        public IActionResult LeaveProject([FromBody] LeaveProjectModel model)
        {
            Console.WriteLine($"LeaveProjectModel: {model.ProjectId} {model.UserId}");
            m_ProjectService.LeaveProject(model);

            return Ok(new
            {
                Left = true
            });

        }

        [HttpPost("addmember")]
        public IActionResult AddMember([FromBody] AddMemberModel model)
        {
            Console.WriteLine($"LeaveProjectModel: {model.ProjectId} {model.Name}");
            var result = m_ProjectService.AddMember(model);

            return Ok(new
            {
                Response = result
            });

        }

        [HttpGet("getcard")]
        public async Task<ActionResult<GetCardReturnJson>> GetCardMarkdown(int cardId)
        {
            Console.WriteLine($"GetCardReturnJson: {cardId}");
            GetCardReturnJson json = await m_ProjectService.GetCardMarkdown(cardId);
            return json;

        }

        [HttpGet("getprojectmembers")]
        public ActionResult<GetProjectMembers> GetProjectMembers(int projectId)
        {
            Console.WriteLine($"GetProjectMembers: {projectId}");
            GetProjectMembers json = m_ProjectService.GetProjectMembers(projectId);
            return json;

        }
    }
}
