using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
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
        private readonly IImportService m_ImportService;
        private readonly HomeService m_HomeService;

        public HomeController(ApplicationDbContext context, ILogger<HomeController> logger, UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, LocalisationService localisationService, AuditService auditService, IEmailSender emailSender, ProjectService projectService, ICaptchaService captchaService, IImportService importService, HomeService homeService)
        {
            m_Database = context;
            m_Logger = logger;
            m_UserManager = userManager;
            m_SignInManager = signInManager;
            m_Localisation = localisationService;
            m_AuditService = auditService;
            m_EmailSender = emailSender;
            m_ProjectService = projectService;
            m_CaptchaService = captchaService;
            m_ImportService = importService;
            m_HomeService = homeService;
        }

        public async Task<IActionResult> Index(int orgId = 0)
        {
            CommonCookies.ApplyCommonCookies(HttpContext);

            HomeViewModel viewModel = new HomeViewModel();
            if (m_SignInManager.IsSignedIn(User))
            {
                var user = await m_UserManager.GetUserAsync(User);
                if(user != null)
                {
                    JoinController.DoFinishedRegistrationChecks(this, user);
                    user.LastVisited = DateTime.Now;
                    await m_UserManager.UpdateAsync(user);
                    viewModel = await m_HomeService.GetHomeViewModelAsync(user, orgId);
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

            var project = await m_HomeService.CreateProjectAsync(appUser, createProject);

            await m_AuditService.InsertLogAsync(AuditLogCategory.Project, appUser, AuditLogType.Created, project.Id);
            await m_Database.SaveChangesAsync();

            return RedirectToAction(nameof(ProjectController.Index), "Project", new { id = project.Id });
        }

        [HttpPost]
        public async Task<IActionResult> Edit(HomeViewModel.EditProjectModel editProject)
        {
            if (!m_SignInManager.IsSignedIn(User))
            {
                return RedirectToAction(nameof(Index));
            }

            var appUser = await m_UserManager.GetUserAsync(User);
            if(appUser == null)
            {
                m_Logger.LogError($"WARNING: Account {m_UserManager.GetUserId(User)} returned null");
                return RedirectToAction(nameof(Index));
            }

            var project = await m_HomeService.EditProjectAsync(appUser, editProject);
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Delete(HomeViewModel.DeleteProjectModel deleteProject)
        {
            if (!m_SignInManager.IsSignedIn(User))
            {
                return RedirectToAction(nameof(Index));
            }

            var appUser = await m_UserManager.GetUserAsync(User);
            if (appUser == null)
            {
                m_Logger.LogError($"WARNING: Account {m_UserManager.GetUserId(User)} returned null");
                return RedirectToAction(nameof(Index));
            }
            await m_HomeService.DeleteProjectAsync(appUser, deleteProject);

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

        [HttpPost]
        public async Task<IActionResult> ImportTrello(HomeViewModel.ImportTrelloModel importTrello)
        {
            if(!ModelState.IsValid)
            {
                m_Logger.LogError($"Returned bad model state during TrelloImport: {ModelState}");
                return BadRequest(ModelState);
            }

            if(User == null || User.Identity == null)
            {
                m_Logger.LogError($"User or User.Identity was null during TrelloImport");
                return BadRequest("User was null");
            }

            var appUser = await m_UserManager.GetUserAsync(User);
            if (appUser == null)
            {
                m_Logger.LogError($"appUser was null during TrelloImport");
                return BadRequest("appUser was null");
            }

            var trelloBoard = m_ImportService.ImportTrelloJson(appUser, importTrello.Json);

            var project = new Project();
            project.Name = trelloBoard.Name;
            project.Description = trelloBoard.Description;
            project.CreatedDate = DateTime.Now;
            project.DueDate = null;
            project.OwnerId = Guid.Parse(m_UserManager.GetUserId(User));
            project.OrganisationId = 0;

            await m_Database.Projects.AddAsync(project);
            await m_Database.SaveChangesAsync();

            await m_AuditService.InsertLogAsync(AuditLogCategory.Project, appUser, AuditLogType.ImportedTrello, project.Id);
            await m_Database.SaveChangesAsync();

            m_Logger.LogInformation($"Trello Import: Created project {project.Name}");

            foreach (var column in trelloBoard.Lists)
            {
                int colId = await m_ProjectService.AddColumnAsync(project.Id, column.Name);
                m_Logger.LogInformation($"Trello Import: Created column {column.Name}");

                foreach(var card in column.Cards)
                {
                    int cardId = await m_ProjectService.AddTask(User, colId, card.Name, card.Description, card.Due);
                    card.CardId = cardId;

                    foreach(var checklist in card.Checklists)
                    {
                        AddChecklistModel addChecklist = new AddChecklistModel();
                        addChecklist.Id = cardId;
                        addChecklist.Name = checklist.Name;
                        var newChecklist = await m_ProjectService.AddChecklist(addChecklist, User);

                        foreach(var checklistItem in checklist.ChecklistItems)
                        {
                            AddChecklistItemModel addChecklistItem = new AddChecklistItemModel();
                            addChecklistItem.ChecklistId = newChecklist.Id;
                            addChecklistItem.ItemText = checklistItem.Name;
                            addChecklistItem.Completed = checklistItem.Complete;

                            await m_ProjectService.AddChecklistItem(addChecklistItem, User);
                        }
                        m_Logger.LogInformation($"Trello Import: Created {checklist.ChecklistItems.Count} checklistItems in checklist {checklist.Name}");
                    }
                    m_Logger.LogInformation($"Trello Import: Created {card.Checklists.Count} checklists in task {card.Name}");
                }
                m_Logger.LogInformation($"Trello Import: Created {column.Cards.Count} tasks in column {column.Name}");
            }
            m_Logger.LogInformation($"Trello Import: Account {m_UserManager.GetUserId(User)} successfully created {project.Id}");
            return RedirectToAction(nameof(ProjectController.Index), "Project", new { id = project.Id });
            //return RedirectToAction("Index", "Project", new { id = project.Id });
        }
    }
}