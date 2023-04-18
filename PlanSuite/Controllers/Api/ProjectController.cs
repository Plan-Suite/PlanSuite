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
        public async Task<IActionResult> MoveCard([FromBody] MoveCardModel model)
        {
            Console.WriteLine($"MoveCard: {model.CardId} {model.ColumnId}");
            await m_ProjectService.MoveCard(model, User);

            return Ok(new
            {
                Moved = true
            });

        }

        [HttpPost("editcarddesc")]
        public async Task<IActionResult> EditCardDesc([FromBody] EditCardDescModel model)
        {
            Console.WriteLine($"EditCardDescModel: {model.CardId} {model.Description}");
            await m_ProjectService.EditCardDesc(model, User);

            return Ok(new
            {
                Modified = true
            });

        }

        [HttpPost("editcardname")]
        public async Task<IActionResult> EditCardDesc([FromBody] EditCardNameModel model)
        {
            Console.WriteLine($"EditCardNameModel: {model.CardId} {model.Name}");
            await m_ProjectService.EditCardName(model, User);

            return Ok(new
            {
                Modified = true
            });

        }

        [HttpPost("EditCard")]
        public async Task<IActionResult> EditCardAsync([FromBody] EditCardModel model)
        {
            Console.WriteLine($"EditCardModel: {model.CardId} {model.Timestamp} {model.Priority} {model.AssigneeId} {model.Budget}");
            await m_ProjectService.EditCardAsync(model, User);

            return Ok(new
            {
                Modified = true
            });

        }

        [HttpPost("leaveproject")]
        public async Task<IActionResult> LeaveProject([FromBody] LeaveProjectModel model)
        {
            Console.WriteLine($"LeaveProjectModel: {model.ProjectId} {model.UserId}");
            await m_ProjectService.LeaveProject(model);

            return Ok(new
            {
                Left = true
            });

        }

        [HttpGet("getcard")]
        public async Task<ActionResult<GetCardReturnJson>> GetCardMarkdown(int cardId)
        {
            Console.WriteLine($"GetCardReturnJson: {cardId}");
            GetCardReturnJson json = await m_ProjectService.GetCardMarkdown(cardId);
            return json;

        }

        /*[HttpGet("getcards")]
        public ActionResult<GetCardsModel> GetCards(int projectId)
        {
            Console.WriteLine($"GetCardsModel: {projectId}");
            GetCardsModel json = m_ProjectService.GetCards(projectId);
            return json;

        }*/

        [HttpGet("getprojectmembers")]
        public ActionResult<GetProjectMembers> GetProjectMembers(int projectId)
        {
            Console.WriteLine($"GetProjectMembers: {projectId}");
            GetProjectMembers json = m_ProjectService.GetProjectMembers(projectId);
            return json;

        }

        [HttpPost("EditColumnName")]
        public async Task<IActionResult> EditColumnName([FromBody] EditColumnNameModel model)
        {
            Console.WriteLine($"EditColumnName: {model.ColumnId} {model.ColumnText}");
            await m_ProjectService.EditColumnTitle(model, User);

            return Ok(new
            {
                Modified = true
            });

        }

        [HttpPost("AddChecklistItem")]
        public async Task<ActionResult<ChecklistItem>> AddChecklistItem([FromBody] AddChecklistItemModel model)
        {
            Console.WriteLine($"AddChecklistItem: {model.ChecklistId} {model.ItemText}");
            return await m_ProjectService.AddChecklistItem(model, User);
        }

        [HttpPost("EditChecklistItemTickedState")]
        public async Task<IActionResult> EditChecklistItemTickedState([FromBody] EditChecklistItemTickedStateModel model)
        {
            Console.WriteLine($"EditChecklistItemTickedState: {model.ChecklistItemId} {model.TickedState}");
            await m_ProjectService.EditChecklistItemTickedState(model, User);

            return Ok(new
            {
                Modified = true
            });
        }

        [HttpPost("ConvertChecklistItemToCard")]
        public async Task<IActionResult> ConvertChecklistItemToCard([FromBody] ConvertChecklistItemModel model)
        {
            Console.WriteLine($"ConvertChecklistItemToCard: {model.ChecklistItemId}");
            bool result = await m_ProjectService.ConvertChecklistItemToCard(model, User);

            return Ok(new
            {
                Converted = result
            });
        }

        [HttpPost("DeleteChecklistItem")]
        public async Task<IActionResult> DeleteChecklistItem([FromBody] DeleteChecklistItemModel model)
        {
            Console.WriteLine($"DeleteChecklistItemModel: {model.ChecklistItemId}");
            bool result = await m_ProjectService.DeleteChecklistItem(model, User);

            return Ok(new
            {
                Converted = result
            });
        }

        // POST: DeleteChecklist
        [HttpPost("DeleteChecklist")]
        public async Task<IActionResult> DeleteChecklistItem([FromBody] DeleteChecklistModel model)
        {
            Console.WriteLine($"DeleteChecklist: {model.ChecklistId}");
            bool result = await m_ProjectService.DeleteChecklist(model, User);

            return Ok(new
            {
                Deleted = result
            });
        }

        // POST: AddChecklist
        [HttpPost("AddChecklist")]
        public async Task<ActionResult<CardChecklist>> AddChecklist([FromBody] AddChecklistModel model)
        {
            Console.WriteLine($"AddChecklist: {model.Id} {model.Name}");
            return await m_ProjectService.AddChecklist(model, User);
        }

        // GET: GetMilestoneInfoForEditing
        [HttpGet("GetMilestoneInfoForEditing")]
        public async Task<ActionResult<GetMilestoneDataForEditingModel>> GetMilestoneInfoForEditingAsync(int id)
        {
            Console.WriteLine($"GetMilestoneInfoForEditing: {id}");
            return m_ProjectService.GetMilestoneInfoForEditingAsync(id);
        }

        // POST: ToggleMilestoneIsClosed
        [HttpPost("ToggleMilestoneIsClosed")]
        public async Task<ActionResult<GetToggleMilestoneIsClosedModel>> ToggleMilestoneIsClosed([FromBody] ToggleMilestoneIsClosedModel model)
        {
            Console.WriteLine($"ToggleMilestoneIsClosed: {model.MilestoneId}");
            return await m_ProjectService.ToggleMilestoneIsClosedAsync(model, User);
        }

        // GET: GetMilestones
        [HttpGet("GetMilestones")]
        public ActionResult<GetMilestonesModel> GetMilestones(int id)
        {
            Console.WriteLine($"GetMilestones: {id}");
            return m_ProjectService.GetMilestones(id);
        }

        // GET: GetChartData
        [HttpGet("GetChartData")]
        public ActionResult<ChartViewModel> GetChartData(int id)
        {
            Console.WriteLine($"GetChartData: {id}");
            return m_ProjectService.GetChartData(id);
        }

        /// <summary>
        /// Get a list of calendar tasks by referencing the project id, and optionally filtering by a start date, end date and team member.
        /// </summary>
        /// <param name="id">Project Id</param>
        /// <param name="start">Start Date</param>
        /// <param name="end">End Date</param>
        /// <param name="teamMember">Team Member to only show tasks from</param>
        /// <returns></returns>
        [HttpPost("GetCalendarTasks")]
        public async Task<List<GetCalendarTasksModel.CalendarTask>> GetCalendarTasksAsync([FromForm] int id, [FromForm] Guid teamMember, string? start = null, string? end = null)
        {
            Console.WriteLine($"GetCalendarTasks: {id} (teamMember: {teamMember})");
            return await m_ProjectService.GetCalendarTasksAsync(id, teamMember, start, end);
        }

        // POST: EditTaskDates
        [HttpPost("EditTaskDates")]
        public async Task EditTaskDatesAsync([FromForm] int id, string newStartDate, string newDueDate)
        {
            Console.WriteLine($"EditTaskDates: {id}, {newStartDate}, {newDueDate}");
            await m_ProjectService.EditTaskDates(id, newStartDate, newDueDate);
        }
    }
}
