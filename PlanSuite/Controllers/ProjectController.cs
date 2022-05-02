﻿using Microsoft.AspNetCore.Identity;
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

            Console.WriteLine(JsonSerializer.Serialize(addColumn));

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
    }
}
