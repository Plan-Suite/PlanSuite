using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using PlanSuite.Data;
using PlanSuite.Enums;
using PlanSuite.Interfaces;
using PlanSuite.Migrations;
using PlanSuite.Models.Persistent;
using PlanSuite.Models.Temporary;
using PlanSuite.Services;
using PlanSuite.Utility;
using Stripe;
using Stripe.BillingPortal;
using System.Diagnostics;
using System.Security.Claims;

namespace PlanSuite.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext m_Database;
        private readonly ILogger<HomeController> m_Logger;
        private readonly SignInManager<ApplicationUser> m_SignInManager;
        private readonly UserManager<ApplicationUser> m_UserManager;
        private readonly LocalisationService m_Localisation;
        private readonly AuditService m_AuditService;
        private readonly IEmailSender m_EmailSender;
        private readonly ProjectService m_ProjectService;
        private readonly ICaptchaService m_CaptchaService;

        public HomeController(ApplicationDbContext context, ILogger<HomeController> logger, UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, AuditService auditService, IEmailSender emailSender, ProjectService projectService, ICaptchaService captchaService)
        {
            m_Database = context;
            m_Logger = logger;
            m_UserManager = userManager;
            m_SignInManager = signInManager;
            m_Localisation = LocalisationService.Instance;
            m_AuditService = auditService;
            m_EmailSender = emailSender;
            m_ProjectService = projectService;
            m_CaptchaService = captchaService;
        }

        public async Task<IActionResult> Index(int orgId = 0)
        {
            CommonCookies.ApplyCommonCookies(HttpContext);

            HomeViewModel viewModel = new HomeViewModel();
            if (m_SignInManager.IsSignedIn(User))
            {
                viewModel.DueTasks = new List<Models.Persistent.Card>();
                viewModel.OwnedProjects = new List<HomeViewModel.ProjectModel>();
                Guid userId = Guid.Parse(m_UserManager.GetUserId(User));
                viewModel.CreateOrganisation.OwnerId = userId;

                var user = await m_UserManager.FindByIdAsync(userId.ToString());
                if(user != null)
                {
                    JoinController.DoFinishedRegistrationChecks(this, user);
                    user.LastVisited = DateTime.Now;
                    await m_UserManager.UpdateAsync(user);
                }

                if (orgId >= 1)
                {
                    m_Logger.LogInformation($"Grabbing projects for organisation {orgId} for user {userId}");

                    // Validate if they are actually in said organisation.

                    var orgMember = m_Database.OrganizationsMembership.Where(member => 
                        member.UserId == userId &&
                        member.OrganisationId == orgId &&
                        member.Role >= ProjectRole.User).FirstOrDefault();
                    if(orgMember == null)
                    {
                        m_Logger.LogWarning($"User is not a member of organisation {orgId}");
                        return RedirectToAction(nameof(Index));
                    }

                    var organisation = m_Database.Organizations.Where(org => org.Id == orgId).FirstOrDefault();
                    if(organisation == null)
                    {
                        m_Logger.LogWarning($"Organisation {orgId} does not exist");
                        return RedirectToAction(nameof(Index));
                    }

                    viewModel.Organisations.Add(new ItemList()
                    {
                        Name = organisation.Name,
                        Value = organisation.Id
                    });

                    // Show organisation projects
                    m_Logger.LogInformation($"Grabbing organisation projects for organisation {orgId} for user {userId}");
                    var organisationProjects = m_Database.Projects.Where(p => p.OrganisationId == orgId).ToList();
                    if (organisationProjects != null && organisationProjects.Count > 0)
                    {
                        foreach(var project in organisationProjects)
                        {
                            HomeViewModel.ProjectModel model = new HomeViewModel.ProjectModel
                            {
                                Id = project.Id,
                                Name = project.Name,
                                Description = project.Description,
                                CreatedDate = project.CreatedDate,
                                DueDate = project.DueDate,
                                Client = project.Client,
                                Budget = project.Budget,
                                ProjectBudgetType = (int)project.BudgetType,
                                BudgetMonetaryUnit = project.BudgetMonetaryUnit,
                                ProjectUsedBudget = m_ProjectService.GetUsedBudget(project.Id),
                                OrganisationId = organisation.Id,
                                OrganisationName = organisation.Name,
                                ProjectComplete = project.ProjectCompleted
                            };
                            viewModel.MemberProjects.Add(model);
                        }
                    }
                    m_Logger.LogInformation($"Added {organisationProjects.Count} projects to user {userId} viewModel");

                    viewModel.ViewingOrganisation = organisation;
                    viewModel.CurrentOrganisationMembership = orgMember;
                    viewModel.CreateProject.OrganisationId = orgId;

                    return View(viewModel);
                }

                m_Logger.LogInformation($"Grabbing projects for user {userId}");
                // Get projects where user is owner
                var ownedProjects = m_Database.Projects.Where(p => p.OwnerId == userId && p.OrganisationId < 1).ToList();
                if (ownedProjects != null && ownedProjects.Count > 0)
                {
                    m_Logger.LogInformation($"Grabbing {ownedProjects.Count} owned projects for user {userId}");
                    foreach (var project in ownedProjects)
                    {
                        Organisation organisation = null;
                        if (project.OrganisationId > 0)
                        {
                            organisation = await m_Database.Organizations.Where(org => org.Id == project.OrganisationId).FirstOrDefaultAsync();
                        }

                        HomeViewModel.ProjectModel model = new HomeViewModel.ProjectModel
                        {
                            Id = project.Id,
                            Name = project.Name,
                            Description = project.Description,
                            CreatedDate = project.CreatedDate,
                            DueDate = project.DueDate,
                            Client = project.Client,
                            Budget = project.Budget,
                            ProjectBudgetType = (int)project.BudgetType,
                            BudgetMonetaryUnit = project.BudgetMonetaryUnit,
                            ProjectUsedBudget = m_ProjectService.GetUsedBudget(project.Id),
                            ProjectComplete = project.ProjectCompleted
                        };

                        if (organisation != null)
                        {
                            model.OrganisationId = organisation.Id;
                            model.OrganisationName = organisation.Name;
                        }
                        viewModel.OwnedProjects.Add(model);
                    }

                    m_Logger.LogInformation($"Grabbing unowned due tasks for user {userId}");
                    foreach(var project in ownedProjects)
                    {
                        var columns = m_Database.Columns.Where(c => c.ProjectId == project.Id).ToList();
                        if (columns != null && columns.Count > 0)
                        {
                            foreach (var column in columns)
                            {
                                var cards = m_Database.Cards.Where(card => card.ColumnId == column.Id && card.IsFinished == false).ToList();
                                if (cards != null && cards.Count > 0)
                                {
                                    foreach (var card in cards)
                                    {
                                        if (card.CardDueDate != null && card.CardDueDate <= DateTime.Now.AddMonths(1))
                                        {
                                            viewModel.DueTasks.Add(card);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                // Get projects where user is member
                var projectAccesses = m_Database.ProjectsAccess.Where(access => access.UserId == userId).ToList();
                if (projectAccesses != null && projectAccesses.Count > 0)
                {
                    m_Logger.LogInformation($"Grabbing {projectAccesses.Count} member projects for user {userId}");
                    foreach (var access in projectAccesses)
                    {
                        var project = m_Database.Projects.Where(project => project.Id == access.ProjectId && access.ProjectRole >= ProjectRole.User).FirstOrDefault();
                        if (project != null)
                        {
                            Organisation organisation = null;
                            if(project.OrganisationId > 0)
                            {
                                organisation = await m_Database.Organizations.Where(org => org.Id == project.OrganisationId).FirstOrDefaultAsync();
                            }

                            HomeViewModel.ProjectModel model = new HomeViewModel.ProjectModel
                            {
                                Id = project.Id,
                                Name = project.Name,
                                Description = project.Description,
                                CreatedDate = project.CreatedDate,
                                DueDate = project.DueDate,
                                Client = project.Client,
                                Budget = project.Budget,
                                ProjectBudgetType = (int)project.BudgetType,
                                BudgetMonetaryUnit = project.BudgetMonetaryUnit,
                                ProjectUsedBudget = m_ProjectService.GetUsedBudget(project.Id),
                                ProjectComplete = project.ProjectCompleted
                            };

                            if(organisation != null)
                            {
                                model.OrganisationId = organisation.Id;
                                model.OrganisationName = organisation.Name;
                            }

                            viewModel.MemberProjects.Add(model);

                            m_Logger.LogInformation($"Grabbing due tasks for user {userId} for project {project.Id}");
                            var columns = m_Database.Columns.Where(c => c.ProjectId == project.Id).ToList();
                            if (columns != null && columns.Count > 0)
                            {
                                foreach (var column in columns)
                                {
                                    var cards = m_Database.Cards.Where(card => card.ColumnId == column.Id && card.IsFinished == false).ToList();
                                    if (cards != null && cards.Count > 0)
                                    {
                                        foreach (var card in cards)
                                        {
                                            if (card.CardDueDate != null && card.CardDueDate <= DateTime.Now.AddMonths(1))
                                            {
                                                viewModel.DueTasks.Add(card);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                // get all organisations user is member of and then return said organisation owned projects projects
                var organisationMemberships = m_Database.OrganizationsMembership.Where(member => member.UserId == userId).ToList();
                if (organisationMemberships != null && organisationMemberships.Count > 0)
                {
                    m_Logger.LogInformation($"Grabbing organisation projects for {organisationMemberships.Count} organisations for user {userId}");
                    foreach (var organisationMemership in organisationMemberships)
                    {
                        int organisationId = organisationMemership.OrganisationId;
                        var organisation = m_Database.Organizations.Where(o => o.Id == organisationId).FirstOrDefault();
                        if (organisation != null)
                        {
                            // Confirm if user is actually a member of said organisation
                            var organisationMembership = m_Database.OrganizationsMembership.Where(member => member.OrganisationId == organisationId && member.UserId == userId && member.Role >= ProjectRole.User).FirstOrDefault();
                            if (organisationMembership != null)
                            {
                                if(organisationMembership.Role >= ProjectRole.Admin)
                                {
                                    viewModel.Organisations.Add(new ItemList()
                                    {
                                        Name = organisation.Name,
                                        Value = organisation.Id
                                    });
                                }
                                var organisationProjects = m_Database.Projects.Where(p => p.OrganisationId == organisationId).ToList();
                                if (organisationProjects != null && organisationProjects.Count > 0)
                                {
                                    foreach (var project in organisationProjects)
                                    {
                                        m_Logger.LogInformation($"Grabbing organisation due tasks for user {userId} for organisation {organisation.Id} project {project.Id}");
                                        var columns = m_Database.Columns.Where(c => c.ProjectId == project.Id).ToList();
                                        if (columns != null && columns.Count > 0)
                                        {
                                            foreach (var column in columns)
                                            {
                                                var cards = m_Database.Cards.Where(card => card.ColumnId == column.Id && card.IsFinished == false).ToList();
                                                if (cards != null && cards.Count > 0)
                                                {
                                                    foreach (var card in cards)
                                                    {
                                                        if (card.CardDueDate != null && card.CardDueDate <= DateTime.Now.AddMonths(1))
                                                        {
                                                            viewModel.DueTasks.Add(card);
                                                        }
                                                    }
                                                }
                                            }
                                        }

                                        HomeViewModel.ProjectModel model = new HomeViewModel.ProjectModel
                                        {
                                            Id = project.Id,
                                            Name = project.Name,
                                            Description = project.Description,
                                            CreatedDate = project.CreatedDate,
                                            DueDate = project.DueDate,
                                            Client = project.Client,
                                            Budget = project.Budget,
                                            ProjectBudgetType = (int)project.BudgetType,
                                            BudgetMonetaryUnit = project.BudgetMonetaryUnit,
                                            ProjectUsedBudget = m_ProjectService.GetUsedBudget(project.Id),
                                            OrganisationId = organisation.Id,
                                            OrganisationName = organisation.Name,
                                            ProjectComplete = project.ProjectCompleted
                                        };

                                        viewModel.MemberProjects.Add(model);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return View(viewModel);
        }

        public IActionResult Features()
        {
            return View();
        }

        [Route("pricing")]
        public IActionResult Pricing()
        {
            return View();
        }

        [Route("contact-sales")]
        public IActionResult Sales()
        {
            var model = new ContactSalesViewModel();
            return View(model);
        }

        public IActionResult SalesContacted()
        {
            return View();
        }

        [Route("about-us")]
        public IActionResult AboutUs()
        {
            return View();
        }

        [Route("jobs")]
        public IActionResult Careers()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> OnSalesContact(ContactSalesViewModel.ContactSalesModel contactSales)
        {
            if(!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }


            string jsonString = System.Text.Json.JsonSerializer.Serialize(contactSales);
            Console.WriteLine(jsonString);

            var captchaResult = m_CaptchaService.Verify(contactSales.Token);
            if(captchaResult.Result.Success == false && captchaResult.Result.Score <= 0.5)
            {
                return RedirectToAction(nameof(SalesContacted));
            }

            // Add sales contact to database
            var salesContact = new SalesContact();
            salesContact.FirstName = contactSales.FirstName;
            salesContact.LastName = contactSales.LastName;
            salesContact.Email = contactSales.Email;

            if(!string.IsNullOrEmpty(salesContact.PhoneNumber))
            {
                salesContact.PhoneNumber = contactSales.PhoneNumber;
            }
            else
            {
                salesContact.PhoneNumber = "N/A";
            }

            if (!string.IsNullOrEmpty(salesContact.JobTitle))
            {
                salesContact.JobTitle = contactSales.JobTitle;
            }
            else
            {
                salesContact.JobTitle = "N/A";
            }

            salesContact.Message = contactSales.Message;
            salesContact.Timestamp = DateTime.Now;
            salesContact.IsContacted = false;

            await m_Database.SalesContacts.AddAsync(salesContact);
            await m_Database.SaveChangesAsync();

            return RedirectToAction(nameof(SalesContacted));
        }

        [HttpPost]
        public async Task<IActionResult> Create(HomeViewModel.CreateProjectModel createProject)
        {
            if (!m_SignInManager.IsSignedIn(User))
            {
                return RedirectToAction(nameof(Index));
            }

            ClaimsPrincipal claimsPrincipal = HttpContext.User as ClaimsPrincipal;

            if(string.IsNullOrEmpty(createProject.Name))
            {
                return RedirectToAction(nameof(Index));
            }

            if (string.IsNullOrEmpty(createProject.Description))
            {
                createProject.Description = createProject.Name;
            }

            var appUser = await m_UserManager.GetUserAsync(User);
            if(appUser == null)
            {
                return BadRequest();
            }

            var project = new Project();
            project.Name = createProject.Name;
            project.Description = createProject.Description;
            project.CreatedDate = DateTime.Now;
            project.DueDate = createProject.DueDate;
            project.OwnerId = Guid.Parse(m_UserManager.GetUserId(claimsPrincipal));
            project.OrganisationId = createProject.OrganisationId;
            if(!string.IsNullOrEmpty(createProject.Client))
            {
                project.Client = createProject.Client;
            }
            if(createProject.Budget > 0.0m && appUser.PaymentTier >= PaymentTier.Plus)
            {
                project.Budget = createProject.Budget;
                project.BudgetType = createProject.BudgetType;
                if(project.BudgetType == ProjectBudgetType.Cost)
                {
                    project.BudgetMonetaryUnit = createProject.BudgetUnit;
                }
            }
            await m_Database.Projects.AddAsync(project);
            await m_Database.SaveChangesAsync();

            await m_AuditService.InsertLogAsync(AuditLogCategory.Project, appUser, AuditLogType.Created, project.Id);
            await m_Database.SaveChangesAsync();

            m_Logger.LogInformation($"Account {m_UserManager.GetUserId(claimsPrincipal)} successfully created {project.Id}");

            return RedirectToAction(nameof(ProjectController.Index), "Project", new { id = project.Id });
        }

        [HttpPost]
        public async Task<IActionResult> Edit(HomeViewModel.EditProjectModel editProject)
        {
            if (!m_SignInManager.IsSignedIn(User))
            {
                return RedirectToAction(nameof(Index));
            }

            ClaimsPrincipal claimsPrincipal = HttpContext.User as ClaimsPrincipal;
            var project = m_Database.Projects.FirstOrDefault(p => p.Id == editProject.Id);
            if(project == null)
            {
                m_Logger.LogError($"No project with id {editProject.Id} found");
                return RedirectToAction(nameof(Index));
            }

            string userId = m_UserManager.GetUserId(claimsPrincipal);
            if (project.OwnerId != Guid.Parse(userId))
            {
                m_Logger.LogError($"WARNING: Account {m_UserManager.GetUserId(claimsPrincipal)} tried to modify {project.Id} without correct permissions");
                return RedirectToAction(nameof(Index));
            }

            var appUser = await m_UserManager.FindByIdAsync(userId);
            if(appUser == null)
            {
                m_Logger.LogError($"WARNING: Account {m_UserManager.GetUserId(claimsPrincipal)} returned null");
                return RedirectToAction(nameof(Index));
            }

            m_Logger.LogInformation($"Account {m_UserManager.GetUserId(claimsPrincipal)} successfully modified {project.Id}");
            project.Name = editProject.Name;
            project.Description = editProject.Description;
            project.DueDate = editProject.DueDate;
            project.OrganisationId = editProject.Organisation;
            project.Client = editProject.Client;
            if (editProject.Budget > 0.0m && appUser.PaymentTier >= PaymentTier.Plus)
            {
                project.Budget = editProject.Budget;
                project.BudgetType = editProject.BudgetType;
                if (project.BudgetType == ProjectBudgetType.Cost)
                {
                    project.BudgetMonetaryUnit = editProject.BudgetUnit;
                }
            }
            await m_Database.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public IActionResult Delete(HomeViewModel.DeleteProjectModel deleteProject)
        {
            if (!m_SignInManager.IsSignedIn(User))
            {
                return RedirectToAction(nameof(Index));
            }

            ClaimsPrincipal claimsPrincipal = HttpContext.User as ClaimsPrincipal;
            var project = m_Database.Projects.FirstOrDefault(p => p.Id == deleteProject.Id);
            if (project == null)
            {
                Console.WriteLine($"No project with id {deleteProject.Id} found");
                return RedirectToAction(nameof(Index));
            }

            if(project.OwnerId != Guid.Parse(m_UserManager.GetUserId(claimsPrincipal)))
            {
                Console.WriteLine($"WARNING: Account {m_UserManager.GetUserId(claimsPrincipal)} tried to delete {project.Id} without correct permissions");
                return RedirectToAction(nameof(Index));
            }

            m_Logger.LogInformation($"Account {m_UserManager.GetUserId(claimsPrincipal)} successfully deleted {project.Id}");
            m_Database.Projects.Remove(project);
            m_Database.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        [Route("privacy")]
        public IActionResult Privacy()
        {
            CommonCookies.ApplyCommonCookies(HttpContext);
            return View();
        }

        [Route("terms")]
        public IActionResult License()
        {
            CommonCookies.ApplyCommonCookies(HttpContext);
            return View();
        }

        public IActionResult Changelog()
        {
            CommonCookies.ApplyCommonCookies(HttpContext);
            ChangelogViewModel viewModel = new ChangelogViewModel();
            viewModel.Changelogs = m_Database.ChangeLogs.ToList();
            return View(viewModel);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [Route("/error/{code:int}")]
        public async Task<IActionResult> Error(int code)
        {
            ApplicationUser? user = await m_UserManager.GetUserAsync(User);

            ErrorCode errorCode;
            string errorMessage;
            string errorImage = "/img";
            string errorImageAlt;

            switch (code)
            {
                case 401:
                    errorCode = ErrorCode.Unauthorised;
                    errorMessage = m_Localisation.Get(user, "ERROR_UNAUTHORISED");
                    errorImage += "/lock.png";
                    errorImageAlt = "Padlock";
                    break;
                case 404:
                    errorCode = ErrorCode.PageNotFound;
                    errorMessage = m_Localisation.Get(user, "ERROR_PAGE_NOT_FOUND");
                    errorImage += "/pagenotfound.png";
                    errorImageAlt = "Sad face inside magnifying glass";
                    break;
                case 500:
                    errorCode = ErrorCode.ServerError;
                    errorMessage = m_Localisation.Get(user, "ERROR_SERVER_ERROR");
                    errorImage += "/servererror.png";
                    errorImageAlt = "Cloud with exclamation";
                    break;
                case 502:
                    errorCode = ErrorCode.BadGateway;
                    errorMessage = m_Localisation.Get(user, "ERROR_BAD_GATEWAY");
                    errorImage += "/badgateway.png";
                    errorImageAlt = "Globe connected to computer with error between";
                    break;
                default:
                    errorCode = ErrorCode.Unknown;
                    errorMessage = m_Localisation.Get(user, "ERROR_UNKNOWN");
                    errorImage += "/error.png";
                    errorImageAlt = "Exclamation inside triangle";
                    break;
            }

            if(user != null)
            {
                m_Logger.LogError($"Shown error page for {user.FullName} ({user.Email}) [{DateTime.Now.ToLongTimeString()}]: #{code}: {errorCode}");

            }
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier, ErrorCode = errorCode, ErrorMessage = errorMessage, Code = code, Image = errorImage, ImageAlt = errorImageAlt });
        }

        public IActionResult AuthError()
        {
            CommonCookies.ApplyCommonCookies(HttpContext);
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> BillingPortal()
        {
            if (!m_SignInManager.IsSignedIn(User))
            {
                m_Logger.LogWarning("User was not signed in during BillingPortal");
                return RedirectToAction(nameof(Index));
            }

            var user = await m_UserManager.GetUserAsync(User);
            if (user == null)
            {
                m_Logger.LogError("User was null during BillingPortal");
                return BadRequest();
            }

            var customerService = new CustomerService();
            Customer customer;
            if (string.IsNullOrEmpty(user.StripeCustomerId))
            {
                customer = await PaymentUtils.CreateCustomerAsync(user);

                Console.WriteLine(customer.ToJson());

                user.StripeCustomerId = customer.Id;
                await m_UserManager.UpdateAsync(user);
            }
            else
            {
                customer = await customerService.GetAsync(user.StripeCustomerId);
                if(customer == null)
                {
                    customer = await PaymentUtils.CreateCustomerAsync(user);

                    Console.WriteLine(customer.ToJson());

                    user.StripeCustomerId = customer.Id;
                    await m_UserManager.UpdateAsync(user);
                }
            }

            // Authenticate your user.
            var options = new SessionCreateOptions
            {
                Customer = user.StripeCustomerId,
                ReturnUrl = $"https://{HttpContext.Request.Host}/Identity/Account/Manage",
            };
            var sessionService = new SessionService();
            var session = sessionService.Create(options);

            return Redirect(session.Url);
        }
    }
}