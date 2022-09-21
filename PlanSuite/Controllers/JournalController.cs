using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PlanSuite.Data;
using PlanSuite.Models.Persistent;
using PlanSuite.Models.Temporary;
using PlanSuite.Services;
using PlanSuite.Utility;

namespace PlanSuite.Controllers
{
    public class JournalController : Controller
    {
        private readonly ApplicationDbContext m_Database;
        private readonly ILogger<HomeController> m_Logger;
        private readonly SignInManager<ApplicationUser> m_SignInManager;
        private readonly UserManager<ApplicationUser> m_UserManager;
        private readonly LocalisationService m_Localisation;
        private readonly AuditService m_AuditService;

        public JournalController(ApplicationDbContext context, ILogger<HomeController> logger, UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, AuditService auditService)
        {
            m_Database = context;
            m_Logger = logger;
            m_UserManager = userManager;
            m_SignInManager = signInManager;
            m_Localisation = LocalisationService.Instance;
            m_AuditService = auditService;
        }

        public async Task<IActionResult> Index()
        {
            if(!m_SignInManager.IsSignedIn(User))
            {
                return RedirectToAction(nameof(HomeController.Index), "Home");
            }

            CommonCookies.ApplyCommonCookies(HttpContext);

            var appUser = await m_UserManager.GetUserAsync(User);
            Guid userId = Guid.Parse(appUser.Id);
            var journalNotes = m_Database.JournalNotes.Where(j => j.OwnerId == userId).OrderByDescending(j => j.Modified).ToList();
            JournalIndexViewModel viewModel = new JournalIndexViewModel();
            viewModel.PrivateNotes = journalNotes;

            return View(viewModel);
        }
    }
}
