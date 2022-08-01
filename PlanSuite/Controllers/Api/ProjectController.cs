using Microsoft.AspNetCore.Mvc;
using PlanSuite.Models.Persistent;
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

        [HttpPost("EditColumnName")]
        public IActionResult EditColumnName([FromBody] EditColumnNameModel model)
        {
            Console.WriteLine($"EditColumnName: {model.ColumnId} {model.ColumnText}");
            m_ProjectService.EditColumnTitle(model);

            return Ok(new
            {
                Modified = true
            });

        }

        [HttpPost("AddChecklistItem")]
        public ActionResult<ChecklistItem> AddChecklistItem([FromBody] AddChecklistItemModel model)
        {
            Console.WriteLine($"AddChecklistItem: {model.ChecklistId} {model.ItemText}");
            return m_ProjectService.AddChecklistItem(model);
        }

        [HttpPost("EditChecklistItemTickedState")]
        public IActionResult EditChecklistItemTickedState([FromBody] EditChecklistItemTickedStateModel model)
        {
            Console.WriteLine($"EditChecklistItemTickedState: {model.ChecklistItemId} {model.TickedState}");
            m_ProjectService.EditChecklistItemTickedState(model);

            return Ok(new
            {
                Modified = true
            });
        }

        [HttpPost("ConvertChecklistItemToCard")]
        public IActionResult ConvertChecklistItemToCard([FromBody] ConvertChecklistItemModel model)
        {
            Console.WriteLine($"ConvertChecklistItemToCard: {model.ChecklistItemId}");
            bool result = m_ProjectService.ConvertChecklistItemToCard(model);

            return Ok(new
            {
                Converted = result
            });
        }

        [HttpPost("DeleteChecklistItem")]
        public IActionResult DeleteChecklistItem([FromBody] DeleteChecklistItemModel model)
        {
            Console.WriteLine($"DeleteChecklistItemModel: {model.ChecklistItemId}");
            bool result = m_ProjectService.DeleteChecklistItem(model);

            return Ok(new
            {
                Converted = result
            });
        }

        // POST: DeleteChecklist
        [HttpPost("DeleteChecklist")]
        public IActionResult DeleteChecklistItem([FromBody] DeleteChecklistModel model)
        {
            Console.WriteLine($"DeleteChecklist: {model.ChecklistId}");
            bool result = m_ProjectService.DeleteChecklist(model);

            return Ok(new
            {
                Deleted = result
            });
        }

        // POST: AddChecklist
        [HttpPost("AddChecklist")]
        public ActionResult<CardChecklist> AddChecklist([FromBody] AddChecklistModel model)
        {
            Console.WriteLine($"AddChecklist: {model.Id} {model.Name}");
            return m_ProjectService.AddChecklist(model);
        }

        // GET: GetMilestoneInfoForEditing
        [HttpGet("GetMilestoneInfoForEditing")]
        public ActionResult<GetMilestoneDataForEditingModel> GetMilestoneInfoForEditingAsync(int id)
        {
            Console.WriteLine($"GetMilestoneInfoForEditing: {id}");
            return m_ProjectService.GetMilestoneInfoForEditingAsync(id);
        }

        // POST: ToggleMilestoneIsClosed
        [HttpPost("ToggleMilestoneIsClosed")]
        public async Task<ActionResult<GetToggleMilestoneIsClosedModel>> ToggleMilestoneIsClosed([FromBody] ToggleMilestoneIsClosedModel model)
        {
            Console.WriteLine($"ToggleMilestoneIsClosed: {model.MilestoneId}");
            return await m_ProjectService.ToggleMilestoneIsClosedAsync(model);
        }

        // GET: GetMilestones
        [HttpGet("GetMilestones")]
        public ActionResult<GetMilestonesModel> GetMilestones(int id)
        {
            Console.WriteLine($"GetMilestones: {id}");
            return m_ProjectService.GetMilestones(id);
        }
    }
}
