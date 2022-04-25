using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PlanSuite.Data;
using PlanSuite.Models.Persistent;
using PlanSuite.Models.Temporary;
using System.Security.Claims;

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

            return View(viewModel);
        }
    }
}
