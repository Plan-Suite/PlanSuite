using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PlanSuite.Data;
using PlanSuite.Models.Persistent;
using PlanSuite.Models.Temporary;
using System.Diagnostics;
using System.Security.Claims;

namespace PlanSuite.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext dbContext;
        private readonly ILogger<HomeController> _logger;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;

        public HomeController(ApplicationDbContext context, ILogger<HomeController> logger, UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager)
        {
            dbContext = context;
            _logger = logger;
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public IActionResult Index()
        {
            HomeViewModel viewModel = new HomeViewModel();
            if (_signInManager.IsSignedIn(User))
            {
                ClaimsPrincipal claimsPrincipal = HttpContext.User as ClaimsPrincipal;
                viewModel.Projects = dbContext.Projects.Where(p => p.OwnerId == Guid.Parse(_userManager.GetUserId(claimsPrincipal))).ToList();
            }
            return View(viewModel);
        }

        [HttpPost]
        public IActionResult Create(HomeViewModel.CreateProjectModel createProject)
        {
            if (!_signInManager.IsSignedIn(User))
            {
                return RedirectToAction(nameof(Index));
            }

            ClaimsPrincipal claimsPrincipal = HttpContext.User as ClaimsPrincipal;

            var project = new Project();
            project.Name = createProject.Name;
            project.Description = createProject.Description;
            project.CreatedDate = DateTime.Now;
            project.DueDate = createProject.DueDate;
            project.OwnerId = Guid.Parse(_userManager.GetUserId(claimsPrincipal));
            dbContext.Projects.Add(project);
            dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}