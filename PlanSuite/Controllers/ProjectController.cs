using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlanSuite.Data;
using PlanSuite.Enums;
using PlanSuite.Models.Persistent;
using PlanSuite.Models.Temporary;
using PlanSuite.Services;
using PlanSuite.Utility;
using System.Text.Json; 


namespace PlanSuite.Controllers
{
    public class ProjectController : Controller
    {
        private readonly ApplicationDbContext dbContext;
        private readonly ILogger<HomeController> _logger;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ProjectService m_ProjectService;
        private readonly AuditService m_AuditService;

        public ProjectController(ApplicationDbContext context, ILogger<HomeController> logger, UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, ProjectService projectService, AuditService auditService)
        {
            dbContext = context;
            _logger = logger;
            _userManager = userManager;
            _signInManager = signInManager;
            m_ProjectService = projectService;
            m_AuditService = auditService;
        }

        // /projects/id?filterByTeamMember=X&filterByTaskCompleted=X&filterByTaskOverdue=X&filterByPriority=X
        [Route("projects/{id}")]
        public async Task<IActionResult> Index(int id, Guid? filterByTeamMember = null, int filterByTaskCompleted = 0, int filterByTaskOverdue = 0, int filterByPriority = 0)
        {
            TaskCompletionFilter taskCompleted = (TaskCompletionFilter)filterByTaskCompleted;
            TaskOverdueFilter taskOverdue = (TaskOverdueFilter)filterByTaskOverdue;
            Priority taskPriority = (Priority)filterByPriority;
            CommonCookies.ApplyCommonCookies(HttpContext);
            if (!_signInManager.IsSignedIn(User))
            {
                return RedirectToAction(nameof(Index), "Home");
            }

            var project = await dbContext.Projects.FirstOrDefaultAsync(p => p.Id == id);
            if (project == null)
            {
                Console.WriteLine($"No project with id {id} found");
                return RedirectToAction(nameof(Index), "Home");
            }

            ApplicationUser appUser = await _userManager.GetUserAsync(User);
            var role = m_ProjectService.GetUserProjectAccess(appUser, project);
            if (role == ProjectRole.None)
            {
                Console.WriteLine($"WARNING: Account {_userManager.GetUserId(User)} tried to access {project.Id} without correct permissions");
                return RedirectToAction(nameof(Index), "Home");
            }

            Console.WriteLine($"Account {_userManager.GetUserId(User)} successfully accessed {project.Id} as {role}");
            ProjectViewModel viewModel = new ProjectViewModel();
            viewModel.Project = project;

            var organisation = await dbContext.Organizations.Where(o => o.Id == project.OrganisationId).FirstOrDefaultAsync();
            if(organisation != null)
            {
                viewModel.Organisation = organisation;
            }    
            viewModel.UserId = Guid.Parse(appUser.Id);
            viewModel.ProjectRole = role;

            viewModel.TeamMembers.Add(appUser.UserId, appUser.FullName);
            var users = dbContext.ProjectsAccess.Where(access => access.ProjectId == project.Id).ToList();
            if (users != null)
            {
                foreach (var foundUser in users)
                {
                    var applicationUser = dbContext.Users.Where(user => user.Id == foundUser.UserId.ToString()).FirstOrDefault();
                    if (applicationUser != null)
                    {
                        if (!viewModel.TeamMembers.ContainsKey(applicationUser.UserId))
                        {
                            viewModel.TeamMembers.Add(applicationUser.UserId, applicationUser.FullName);
                        }
                    }
                }
            }

            // Get project owner payment tier
            var user = m_ProjectService.GetProjectOwner(project);
            if(user != null)
            {
                Console.WriteLine($"project {project.Id} owner {user.Id} is on {user.PaymentTier} tier");
                viewModel.PaymentTier = user.PaymentTier;
            }

            var milestones = await dbContext.ProjectMilestones.Where(m => m.ProjectId == project.Id).ToListAsync();
            if (milestones != null && milestones.Count > 0)
            {
                viewModel.Milestones = milestones;
                Console.WriteLine($"Grabbed {milestones.Count} milestones for project {project.Id}");
            }

            var columns = await dbContext.Columns.Where(c => c.ProjectId == project.Id).ToListAsync();
            if(columns != null && columns.Count > 0)
            {
                viewModel.Columns = columns;
                Console.WriteLine($"Grabbed {columns.Count} columns for project {project.Id}");
                foreach (var column in columns)
                {
                    List<Card> cards = await dbContext.Cards.Where(c => c.ColumnId == column.Id).ToListAsync();

                    // Get cards if they have an assignee
                    if (filterByTeamMember != Guid.Empty)
                    {
                        cards = cards.Where(card => card.CardAssignee == filterByTeamMember).ToList();
                    }

                    // Get cards if they have a assignee task completion filter assigned
                    if (taskCompleted == TaskCompletionFilter.Completed)
                    {
                        cards = cards.Where(card => card.IsFinished == true).ToList();
                    }
                    else if (taskCompleted == TaskCompletionFilter.NotCompleted)
                    {
                        cards = cards.Where(card => card.IsFinished == false).ToList();
                    }

                    // Get cards if they have are overdue, also don't show cards that are finished (since they're not overdue)
                    if (taskOverdue == TaskOverdueFilter.Overdue)
                    {
                        cards = cards.Where(card => card.CardDueDate != null && DateTime.Now > card.CardDueDate && card.IsFinished == false).ToList();
                    }
                    else if (taskOverdue == TaskOverdueFilter.NotOverdue)
                    {
                        cards = cards.Where(card => card.CardDueDate != null && DateTime.Now <= card.CardDueDate && card.IsFinished == false).ToList();
                    }

                    if (taskOverdue == TaskOverdueFilter.Overdue)
                    {
                        cards = cards.Where(card => card.CardDueDate != null && DateTime.Now > card.CardDueDate && card.IsFinished == false).ToList();
                    }
                    else if (taskOverdue == TaskOverdueFilter.NotOverdue)
                    {
                        cards = cards.Where(card => card.CardDueDate != null && DateTime.Now <= card.CardDueDate && card.IsFinished == false).ToList();
                    }

                    if (cards != null && cards.Count > 0)
                    {
                        foreach(var card in cards)
                        {
                            if(card.CardStartDate == null)
                            {
                                if(card.CardDueDate != null)
                                {
                                    DateTime pastTime = (DateTime)card.CardDueDate;
                                    card.CardStartDate = pastTime.AddDays(-7);
                                }
                                else
                                {
                                    card.CardStartDate = DateTime.Now;
                                }
                                await dbContext.SaveChangesAsync();
                            }
                            var checklists = await dbContext.CardChecklists.Where(chk => chk.ChecklistCard == card.Id).ToListAsync();
                            foreach(var checklist in checklists)
                            {
                                var checklistItems = await dbContext.ChecklistItems.Where(chkItm => chkItm.ChecklistId == checklist.Id).ToListAsync();
                                foreach(var checklistItem in checklistItems)
                                {
                                    viewModel.ChecklistItems.Add(new ProjectViewModel.ChecklistItemModel
                                    {
                                        ChecklistItemCard = card.Id,
                                        checklistItemTicked = checklistItem.ItemTicked
                                    });
                                }
                            }

                        }
                        viewModel.Cards.AddRange(cards);
                        Console.WriteLine($"Grabbed {cards.Count} cards for project {project.Id}");
                    }
                }
            }

            viewModel.UsedBudget = m_ProjectService.GetUsedBudget(project.Id);

            // TODO: All users will soon be in an organisation, either one they own or their teams organisation.

            // Get project members
            var owner = await _userManager.FindByIdAsync(project.OwnerId.ToString());
            if (owner != null)
            { 
                string userName = owner.Email;
                if (!string.IsNullOrEmpty(owner.FirstName))
                {
                    userName = owner.FullName;
                }
                viewModel.ProjectMembers.Add(project.OwnerId, userName);
            }

            var members = await dbContext.ProjectsAccess.Where(projAccess => projAccess.ProjectId == project.Id).ToListAsync();
            foreach(var member in members)
            {
                var projMember = await _userManager.FindByIdAsync(member.UserId.ToString());
                if(projMember != null)
                {
                    string userName = projMember.Email;
                    if(!string.IsNullOrEmpty(projMember.FirstName))
                    {
                        userName = projMember.FullName;
                    }
                    viewModel.ProjectMembers.Add(member.UserId, userName);
                }
            }

            // Get project members

            if(project.OrganisationId > 0)
            {
                Console.WriteLine($"Getting org members {project.OrganisationId}");
                var memberships = await dbContext.OrganizationsMembership.Where(orgMember => orgMember.OrganisationId == project.OrganisationId).ToListAsync();
                foreach (var member in memberships)
                {
                    Console.WriteLine($"Member {member.UserId}");
                    var orgMember = await _userManager.FindByIdAsync(member.UserId.ToString());
                    if (orgMember != null)
                    {
                        string userName = orgMember.Email;
                        if (!string.IsNullOrEmpty(orgMember.FirstName))
                        {
                            userName = orgMember.FullName;
                        }
                        Console.WriteLine($"OrgMember {userName}");
                        if(!viewModel.OrganisationMembers.ContainsKey(member.UserId))
                        {
                            viewModel.OrganisationMembers.Add(member.UserId, userName);
                        }
                    }
                }
            }
            return View(viewModel);
        }

        // /Project/Logs/?projectId=X&index=Y
        [Route("projects/{projectId}/logs/{index?}")]
        public async Task<IActionResult> Logs(int projectId, int index = 0)
        {
            CommonCookies.ApplyCommonCookies(HttpContext);
            if (!_signInManager.IsSignedIn(User))
            {
                return RedirectToAction(nameof(Index), "Home");
            }

            var project = await dbContext.Projects.FirstOrDefaultAsync(p => p.Id == projectId);
            if (project == null)
            {
                Console.WriteLine($"No project with id {projectId} found");
                return RedirectToAction(nameof(Index), "Home");
            }

            ApplicationUser appUser = await _userManager.GetUserAsync(User);
            var role = m_ProjectService.GetUserProjectAccess(appUser, project);
            if (role == ProjectRole.None)
            {
                Console.WriteLine($"WARNING: Account {_userManager.GetUserId(User)} tried to access {project.Id} without correct permissions");
                return RedirectToAction(nameof(Index), "Home");
            }
            LogViewModel viewModel = new LogViewModel();
            viewModel.Project = project;
            viewModel.AuditLogs = new List<AuditLog>();

            var projectLogs = await dbContext.AuditLogs.Where(log => log.LogCategory == AuditLogCategory.Project && log.TargetID == projectId.ToString()).ToListAsync();
            viewModel.AuditLogs.AddRange(projectLogs);

            var milestones = await dbContext.ProjectMilestones.Where(m => m.ProjectId == projectId).ToListAsync();
            foreach (var milestone in milestones)
            {
                var milestoneLogs = await dbContext.AuditLogs.Where(log => log.LogCategory == AuditLogCategory.Milestone && log.TargetID == milestone.Id.ToString()).ToListAsync();
                viewModel.AuditLogs.AddRange(milestoneLogs);
            }

            var columns = await dbContext.Columns.Where(m => m.ProjectId == projectId).ToListAsync();
            foreach (var column in columns)
            {
                var columnLogs = await dbContext.AuditLogs.Where(log => log.LogCategory == AuditLogCategory.Column && log.TargetID == column.Id.ToString()).ToListAsync();
                viewModel.AuditLogs.AddRange(columnLogs);

                var cards = await dbContext.Cards.Where(card => card.ColumnId == column.Id).ToListAsync();
                foreach (var card in cards)
                {
                    var cardLogs = await dbContext.AuditLogs.Where(log => log.LogCategory == AuditLogCategory.Card && log.TargetID == card.Id.ToString()).ToListAsync();
                    viewModel.AuditLogs.AddRange(cardLogs);
                }
            }

            viewModel.AuditLogs = viewModel.AuditLogs.OrderByDescending(audit => audit.Timestamp).Skip(index - 20).Take(20).ToList();

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> AddColumn(ProjectViewModel.AddColumnModel addColumn)
        {
            if (!_signInManager.IsSignedIn(User))
            {
                return RedirectToAction(nameof(Index), "Home");
            }

            var project = await dbContext.Projects.FirstOrDefaultAsync(p => p.Id == addColumn.ProjectId);
            if (project == null)
            {
                Console.WriteLine($"No project with id {addColumn.ProjectId} found");
                return RedirectToAction(nameof(Index), "Home");
            }

            ApplicationUser appUser = await _userManager.GetUserAsync(User);
            var role = m_ProjectService.GetUserProjectAccess(appUser, project);
            if (role == ProjectRole.None)
            {
                Console.WriteLine($"WARNING: Account {_userManager.GetUserId(User)} tried to access {project.Id} without correct permissions");
                return RedirectToAction(nameof(Index), "Home");
            }

            Console.WriteLine($"Account {_userManager.GetUserId(User)} successfully added a column to {project.Id}");

            int colId = await m_ProjectService.AddColumnAsync(addColumn.ProjectId, addColumn.Name);
            await m_AuditService.InsertLogAsync(AuditLogCategory.Column, appUser, AuditLogType.Added, colId);

            return RedirectToAction(nameof(Index), "Project", new { id = project.Id });
        }

        [HttpPost("addcard")]
        public async Task<IActionResult> AddCard(AddCardModel addCard)
        {
            if (!_signInManager.IsSignedIn(User))
            {
                return RedirectToAction(nameof(Index), "Home");
            }

            Console.WriteLine(JsonSerializer.Serialize(addCard));

            var column = await dbContext.Columns.FirstOrDefaultAsync(p => p.Id == addCard.ColumnId);
            if (column == null)
            {
                Console.WriteLine($"No column with id {addCard.ColumnId} found");
                return RedirectToAction(nameof(Index), "Home");
            }

            Console.WriteLine($"Account {_userManager.GetUserId(User)} successfully added a card to column {column.Id}");

            await m_ProjectService.AddCard(addCard, User);

            return RedirectToAction(nameof(Index), "Project", new { id = column.ProjectId });
        }

        [HttpPost("AddTask")]
        public async Task<IActionResult> AddTask(ProjectViewModel.AddTaskModel addTask)
        {
            if (!_signInManager.IsSignedIn(User))
            {
                return RedirectToAction(nameof(Index), "Home");
            }

            Console.WriteLine(JsonSerializer.Serialize(addTask));

            var column = await dbContext.Columns.FirstOrDefaultAsync(p => p.Id == addTask.ColumnId);
            if (column == null)
            {
                Console.WriteLine($"No column with id {addTask.ColumnId} found");
                return RedirectToAction(nameof(Index), "Home");
            }

            Console.WriteLine($"Account {_userManager.GetUserId(User)} successfully added a card to column {column.Id}");

            await m_ProjectService.AddTask(addTask, User);

            return RedirectToAction(nameof(Index), "Project", new { id = column.ProjectId });
        }

        [HttpPost("AddMember")]
        public async Task<IActionResult> AddMember(ProjectViewModel.AddMemberModel addMember)
        {
            if (!_signInManager.IsSignedIn(User))
            {
                return RedirectToAction(nameof(Index), "Home");
            }

            Console.WriteLine(JsonSerializer.Serialize(addMember));
            var result = await m_ProjectService.AddMember(addMember);
            return RedirectToAction(nameof(Index), "Project", new { id = addMember.ProjectId, addMemberResult = (int)result });
        }

        [HttpPost("addmilestone")]
        public async Task<IActionResult> AddMilestoneAsync(AddMilestoneModel addMilestone)
        {
            if (!_signInManager.IsSignedIn(User))
            {
                return RedirectToAction(nameof(Index), "Home");
            }

            Console.WriteLine(JsonSerializer.Serialize(addMilestone));

            var project = await dbContext.Projects.FirstOrDefaultAsync(p => p.Id == addMilestone.ProjectId);
            if (project == null)
            {
                Console.WriteLine($"No project with id {addMilestone.ProjectId} found");
                return RedirectToAction(nameof(Index), "Home");
            }

            Console.WriteLine($"Account {_userManager.GetUserId(User)} successfully added a milestone to project {project.Id}");

            await m_ProjectService.AddMilestoneAsync(addMilestone, User);

            return RedirectToAction(nameof(Index), "Project", new { id = project.Id });
        }

        [HttpPost("editmilestone")]
        public async Task<IActionResult> EditMilestoneAsync(EditMilestoneModel editMilestone)
        {
            if (!_signInManager.IsSignedIn(User))
            {
                return RedirectToAction(nameof(Index), "Home");
            }

            Console.WriteLine(JsonSerializer.Serialize(editMilestone));

            await m_ProjectService.EditMilestoneAsync(editMilestone, User);

            return RedirectToAction(nameof(Index), "Project", new { id = editMilestone.ProjectId });
        }

        [HttpPost("deletemilestone")]
        public async Task<IActionResult> DeleteMilestoneAsync(DeleteMilestoneModel deleteMilestone)
        {
            if (!_signInManager.IsSignedIn(User))
            {
                return RedirectToAction(nameof(Index), "Home");
            }

            Console.WriteLine(JsonSerializer.Serialize(deleteMilestone));

            await m_ProjectService.DeleteMilestoneAsync(deleteMilestone, User);

            return RedirectToAction(nameof(Index), "Project", new { id = deleteMilestone.ProjectId });
        }

        [HttpPost("MarkComplete")]
        public async Task<IActionResult> MarkCompleteAsync(MarkCompleteModel markComplete)
        {
            if (!_signInManager.IsSignedIn(User))
            {
                return RedirectToAction(nameof(Index), "Home");
            }

            await m_ProjectService.MarkCompleteAsync(markComplete, User);
            return RedirectToAction(nameof(Index), "Project", new { id = markComplete.ProjectId });
        }
    }
}
