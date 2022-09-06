using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PlanSuite.Data;
using PlanSuite.Enums;
using PlanSuite.Migrations;
using PlanSuite.Models.Persistent;
using PlanSuite.Models.Temporary;
using PlanSuite.Services;
using PlanSuite.Utility;
using System.Security.Claims;
using System.Text.Json;

namespace PlanSuite.Controllers
{
    [Route("Project")]
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

        public async Task<IActionResult> Index(int id)
        {
            CommonCookies.ApplyCommonCookies(HttpContext);
            if (!_signInManager.IsSignedIn(User))
            {
                return RedirectToAction(nameof(Index), "Home");
            }

            var project = dbContext.Projects.FirstOrDefault(p => p.Id == id);
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

            var organisation = dbContext.Organizations.Where(o => o.Id == project.OrganisationId).FirstOrDefault();
            if(organisation != null)
            {
                viewModel.Organisation = organisation;
            }    
            viewModel.UserId = Guid.Parse(appUser.Id);
            viewModel.ProjectRole = role;

            // Get project owner payment tier
            var user = m_ProjectService.GetProjectOwner(project);
            if(user != null)
            {
                Console.WriteLine($"project {project.Id} owner {user.Id} is on {user.PaymentTier} tier");
                viewModel.PaymentTier = user.PaymentTier;
            }

            var milestones = dbContext.ProjectMilestones.Where(m => m.ProjectId == project.Id).ToList();
            if (milestones != null && milestones.Count > 0)
            {
                viewModel.Milestones = milestones;
                Console.WriteLine($"Grabbed {milestones.Count} milestones for project {project.Id}");
            }

            var columns = dbContext.Columns.Where(c => c.ProjectId == project.Id).ToList();
            if(columns != null && columns.Count > 0)
            {
                viewModel.Columns = columns;
                Console.WriteLine($"Grabbed {columns.Count} columns for project {project.Id}");
                foreach (var column in columns)
                {
                    var cards = dbContext.Cards.Where(c => c.ColumnId == column.Id).ToList();
                    if (cards != null && cards.Count > 0)
                    {
                        viewModel.Cards.AddRange(cards);
                        Console.WriteLine($"Grabbed {cards.Count} cards for project {project.Id}");
                    }
                }
            }
            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> AddColumn(ProjectViewModel.AddColumnModel addColumn)
        {
            if (!_signInManager.IsSignedIn(User))
            {
                return RedirectToAction(nameof(Index), "Home");
            }

            var project = dbContext.Projects.FirstOrDefault(p => p.Id == addColumn.ProjectId);
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

            var column = new Column();
            column.ProjectId = addColumn.ProjectId;
            column.Title = addColumn.Name;
            await dbContext.Columns.AddAsync(column);
            await dbContext.SaveChangesAsync();
            await m_AuditService.InsertLogAsync(AuditLogCategory.Column, appUser, AuditLogType.Added, column.Id);

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

            var column = dbContext.Columns.FirstOrDefault(p => p.Id == addCard.ColumnId);
            if (column == null)
            {
                Console.WriteLine($"No column with id {addCard.ColumnId} found");
                return RedirectToAction(nameof(Index), "Home");
            }

            Console.WriteLine($"Account {_userManager.GetUserId(User)} successfully added a card to column {column.Id}");

            await m_ProjectService.AddCard(addCard, User);

            return RedirectToAction(nameof(Index), "Project", new { id = column.ProjectId });
        }

        [HttpPost("addmilestone")]
        public async Task<IActionResult> AddMilestoneAsync(AddMilestoneModel addMilestone)
        {
            if (!_signInManager.IsSignedIn(User))
            {
                return RedirectToAction(nameof(Index), "Home");
            }

            Console.WriteLine(JsonSerializer.Serialize(addMilestone));

            var project = dbContext.Projects.FirstOrDefault(p => p.Id == addMilestone.ProjectId);
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
    }
}
