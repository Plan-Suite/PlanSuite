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
        private readonly IWebHostEnvironment m_Environment;

        public JournalController(ApplicationDbContext context, ILogger<HomeController> logger, UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, AuditService auditService, IWebHostEnvironment environment)
        {
            m_Database = context;
            m_Logger = logger;
            m_UserManager = userManager;
            m_SignInManager = signInManager;
            m_Localisation = LocalisationService.Instance;
            m_AuditService = auditService;
            m_Environment = environment;
        }

        [Route("journal/list")]
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

        [Route("journal/{id?}")]
        public async Task<IActionResult> Write(int id = 0)
        {
            if (!m_SignInManager.IsSignedIn(User))
            {
                return RedirectToAction(nameof(HomeController.Index), "Home");
            }

            CommonCookies.ApplyCommonCookies(HttpContext);

            JournalEntryWrite viewModel = new JournalEntryWrite();
            var appUser = await m_UserManager.GetUserAsync(User);
            Guid userId = Guid.Parse(appUser.Id);
            var journalNote = m_Database.JournalNotes.Where(j => j.Id == id && j.OwnerId == userId).OrderByDescending(j => j.Modified).FirstOrDefault();
            if(journalNote != null)
            {
                viewModel.JournalNote = journalNote;
                viewModel.JournalEntryUpdate.JournalId = journalNote.Id;
                viewModel.JournalEntryUpdate.OwnerId = journalNote.OwnerId;
                viewModel.JournalEntryUpdate.Name = journalNote.Name;
                viewModel.JournalEntryUpdate.Content = journalNote.Content;
            }

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Update(JournalEntryUpdateModel journalEntryUpdate)
        {
            if (!m_SignInManager.IsSignedIn(User))
            {
                return RedirectToAction(nameof(Index));
            }

            Guid ownerId = Guid.Parse(m_UserManager.GetUserId(User));

            JournalNote? journalEntry;
            // Journal update
            if (journalEntryUpdate.JournalId > 0 && journalEntryUpdate.OwnerId == ownerId)
            {
                m_Logger.LogInformation($"Updating journal entry for {ownerId}: {journalEntryUpdate.Name}");
                journalEntry = m_Database.JournalNotes.Where(j => j.Id == journalEntryUpdate.JournalId && j.OwnerId == ownerId).FirstOrDefault();
                journalEntry.Modified = DateTime.Now;
                journalEntry.Content = journalEntryUpdate.Content;
                journalEntry.Name = journalEntryUpdate.Name;
            }
            else
            {
                m_Logger.LogInformation($"Creating journal entry for {ownerId}: {journalEntryUpdate.Name}");
                journalEntry = new JournalNote();
                journalEntry.Created = DateTime.Now;
                journalEntry.Modified = journalEntry.Created;
                journalEntry.OwnerId = ownerId;
                journalEntry.Content = journalEntryUpdate.Content;
                journalEntry.Name = journalEntryUpdate.Name;
                await m_Database.JournalNotes.AddAsync(journalEntry);
            }
            await m_Database.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            if (!m_SignInManager.IsSignedIn(User))
            {
                return RedirectToAction(nameof(Index));
            }

            Guid ownerId = Guid.Parse(m_UserManager.GetUserId(User));

            JournalNote? journalEntry = m_Database.JournalNotes.Where(j => j.Id == id && j.OwnerId == ownerId).FirstOrDefault();
            if(journalEntry == null)
            {
                return RedirectToAction(nameof(Index));
            }

            m_Database.JournalNotes.Remove(journalEntry);
            await m_Database.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> UploadImage(IFormFile file)
        {
            string uploadsFolder = Path.Combine(m_Environment.WebRootPath, "uploaded_images");
            if(!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            m_Logger.LogInformation($"Attempting UploadImage {file.FileName} to {uploadsFolder}...");
            var fileName = string.Empty;

            if(file != null && file.Length > 0)
            {
                // TODO: We need to run an antivirus scan on --ANYTHING-- that gets uploaded to the server

                fileName = $"{Guid.NewGuid().ToString()}_{file.FileName}";
                string filePath = Path.Combine(uploadsFolder, fileName);
                using(var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(fileStream);
                }
            }

            return Ok(new { FileUrl = fileName });
        }

        [HttpPost]
        public IActionResult DeleteImage(string file)
        {
            string uploadsFolder = Path.Combine(m_Environment.WebRootPath, "uploaded_images");
            string fileItself = Path.GetFileName(file);
            string filePath = Path.Combine(uploadsFolder, fileItself);

            m_Logger.LogInformation($"Attempting delete image {filePath}...");
            if(System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
                m_Logger.LogInformation($"Deleted image {filePath}");
            }

            return Ok();
        }
    }
}
