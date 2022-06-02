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
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;

        public HomeController(ApplicationDbContext context, ILogger<HomeController> logger, UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
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
                viewModel.OwnedProjects = new List<Project>();

                // Get projects where user is owner
                var ownedProjects = dbContext.Projects.Where(p => p.OwnerId == Guid.Parse(_userManager.GetUserId(User))).ToList();
                viewModel.OwnedProjects.AddRange(ownedProjects);

                // Get projects where user is member
                var projectAccesses = dbContext.ProjectsAccess.Where(access => access.UserId == Guid.Parse(_userManager.GetUserId(User))).ToList();
                foreach(var access in projectAccesses)
                {
                    var project = dbContext.Projects.Where(project => project.Id == access.ProjectId && access.ProjectRole >= Enums.ProjectRole.User).FirstOrDefault();
                    if(project != null)
                    {
                        viewModel.MemberProjects.Add(project);
                    }
                }
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
            Console.WriteLine($"Account {_userManager.GetUserId(claimsPrincipal)} successfully created {project.Id}");

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public IActionResult Edit(HomeViewModel.EditProjectModel editProject)
        {
            if (!_signInManager.IsSignedIn(User))
            {
                return RedirectToAction(nameof(Index));
            }

            ClaimsPrincipal claimsPrincipal = HttpContext.User as ClaimsPrincipal;
            var project = dbContext.Projects.FirstOrDefault(p => p.Id == editProject.Id);
            if(project == null)
            {
                Console.WriteLine($"No project with id {editProject.Id} found");
                return RedirectToAction(nameof(Index));
            }

            if (project.OwnerId != Guid.Parse(_userManager.GetUserId(claimsPrincipal)))
            {
                Console.WriteLine($"WARNING: Account {_userManager.GetUserId(claimsPrincipal)} tried to modify {project.Id} without correct permissions");
                return RedirectToAction(nameof(Index));
            }

            Console.WriteLine($"Account {_userManager.GetUserId(claimsPrincipal)} successfully modified {project.Id}");
            project.Name = editProject.Name;
            project.Description = editProject.Description;
            project.DueDate = editProject.DueDate;
            dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public IActionResult Delete(HomeViewModel.DeleteProjectModel deleteProject)
        {
            if (!_signInManager.IsSignedIn(User))
            {
                return RedirectToAction(nameof(Index));
            }

            ClaimsPrincipal claimsPrincipal = HttpContext.User as ClaimsPrincipal;
            var project = dbContext.Projects.FirstOrDefault(p => p.Id == deleteProject.Id);
            if (project == null)
            {
                Console.WriteLine($"No project with id {deleteProject.Id} found");
                return RedirectToAction(nameof(Index));
            }

            if(project.OwnerId != Guid.Parse(_userManager.GetUserId(claimsPrincipal)))
            {
                Console.WriteLine($"WARNING: Account {_userManager.GetUserId(claimsPrincipal)} tried to delete {project.Id} without correct permissions");
                return RedirectToAction(nameof(Index));
            }

            Console.WriteLine($"Account {_userManager.GetUserId(claimsPrincipal)} successfully deleted {project.Id}");
            dbContext.Projects.Remove(project);
            dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult License()
        {
            return View();
        }

        public IActionResult Changelog()
        {
            ChangelogViewModel viewModel = new ChangelogViewModel();
            viewModel.Changelogs = dbContext.ChangeLogs.ToList();
            return View(viewModel);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}