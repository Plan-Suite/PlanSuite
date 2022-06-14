using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PlanSuite.Data;
using PlanSuite.Enums;
using PlanSuite.Models.Persistent;
using PlanSuite.Models.Temporary;
using PlanSuite.Services;
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

        public ProjectController(ApplicationDbContext context, ILogger<HomeController> logger, UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, ProjectService projectService)
        {
            dbContext = context;
            _logger = logger;
            _userManager = userManager;
            _signInManager = signInManager;
            m_ProjectService = projectService;
        }

        public async Task<IActionResult> Index(int id)
        {
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
            viewModel.UserId = Guid.Parse(appUser.Id);
            viewModel.ProjectRole = role;

            var columns = dbContext.Columns.Where(c => c.ProjectId == project.Id).ToList();
            if(columns != null && columns.Count > 0)
            {
                viewModel.Columns = columns;
                foreach(var column in columns)
                {
                    var cards = dbContext.Cards.Where(c => c.ColumnId == column.Id).ToList();
                    if (cards != null && cards.Count > 0)
                    {
                        viewModel.Cards.AddRange(cards);
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
            dbContext.Columns.Add(column);

            dbContext.SaveChanges();

            return RedirectToAction(nameof(Index), "Project", new { id = project.Id });
        }

        [HttpPost("addcard")]
        public IActionResult AddCard(AddCardModel addCard)
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

            m_ProjectService.AddCard(addCard);

            return RedirectToAction(nameof(Index), "Project", new { id = column.ProjectId });
        }
    }
}
