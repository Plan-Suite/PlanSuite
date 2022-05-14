using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PlanSuite.Data;
using PlanSuite.Models.Persistent;
using PlanSuite.Models.Temporary;
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

        public ProjectController(ApplicationDbContext context, ILogger<HomeController> logger, UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
        {
            dbContext = context;
            _logger = logger;
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public IActionResult Index(int id)
        {
            if (!_signInManager.IsSignedIn(User))
            {
                return RedirectToAction(nameof(Index), "Home");
            }

            ClaimsPrincipal claimsPrincipal = HttpContext.User as ClaimsPrincipal;
            var project = dbContext.Projects.FirstOrDefault(p => p.Id == id);
            if (project == null)
            {
                Console.WriteLine($"No project with id {id} found");
                return RedirectToAction(nameof(Index), "Home");
            }

            if (project.OwnerId != Guid.Parse(_userManager.GetUserId(claimsPrincipal)))
            {
                Console.WriteLine($"WARNING: Account {_userManager.GetUserId(claimsPrincipal)} tried to access {project.Id} without correct permissions");
                return RedirectToAction(nameof(Index), "Home");
            }

            Console.WriteLine($"Account {_userManager.GetUserId(claimsPrincipal)} successfully accessed {project.Id}");
            ProjectViewModel viewModel = new ProjectViewModel();
            viewModel.Project = project;

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
        public IActionResult AddColumn(ProjectViewModel.AddColumnModel addColumn)
        {
            if (!_signInManager.IsSignedIn(User))
            {
                return RedirectToAction(nameof(Index), "Home");
            }

            ClaimsPrincipal claimsPrincipal = HttpContext.User as ClaimsPrincipal;
            var project = dbContext.Projects.FirstOrDefault(p => p.Id == addColumn.ProjectId);
            if (project == null)
            {
                Console.WriteLine($"No project with id {addColumn.ProjectId} found");
                return RedirectToAction(nameof(Index), "Home");
            }

            if (project.OwnerId != Guid.Parse(_userManager.GetUserId(claimsPrincipal)))
            {
                Console.WriteLine($"WARNING: Account {_userManager.GetUserId(claimsPrincipal)} tried to access {project.Id} without correct permissions");
                return RedirectToAction(nameof(Index), "Home");
            }

            Console.WriteLine($"Account {_userManager.GetUserId(claimsPrincipal)} successfully added a column to {project.Id}");

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

            ClaimsPrincipal claimsPrincipal = HttpContext.User as ClaimsPrincipal;
            var column = dbContext.Columns.FirstOrDefault(p => p.Id == addCard.ColumnId);
            if (column == null)
            {
                Console.WriteLine($"No column with id {addCard.ColumnId} found");
                return RedirectToAction(nameof(Index), "Home");
            }

            Console.WriteLine($"Account {_userManager.GetUserId(claimsPrincipal)} successfully added a card to column {column.Id}");

            var card = new Card();
            card.ColumnId = addCard.ColumnId;
            card.CardName = addCard.Name;
            dbContext.Cards.Add(card);

            dbContext.SaveChanges();

            return RedirectToAction(nameof(Index), "Project", new { id = column.ProjectId });
        }
    }
}
