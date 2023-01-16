using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PlanSuite.Data;
using PlanSuite.Interfaces;
using PlanSuite.Models.Persistent;
using PlanSuite.Models.Temporary;
using PlanSuite.Services;
using PlanSuite.Utility;
using System.Diagnostics;
using System.Security.Cryptography.Xml;

namespace PlanSuite.Controllers
{
    // We want to limit access to anything administrator related for obvious reasons
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext dbContext;
        private readonly ILogger<HomeController> _logger;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly AdminService m_AdminService;
        private readonly IEmailSender m_EmailSender;
        private readonly IPathService m_PathService;

        public AdminController(ApplicationDbContext context, ILogger<HomeController> logger, UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, AdminService adminService, IEmailSender emailSender, IPathService pathService)
        {
            dbContext = context;
            _logger = logger;
            _userManager = userManager;
            _signInManager = signInManager;
            m_AdminService = adminService;
            m_EmailSender = emailSender;
            m_PathService = pathService;
        }

        public IActionResult Index()
        {
            if(!User.IsInRole(Constants.AdminRole) && !User.IsInRole(Constants.SupportRole) && !User.IsInRole(Constants.DevRole) && !User.IsInRole(Constants.MarketerRole))
            {
                _logger.LogWarning($"{User.Identity.Name} tried to access ACP->Index");
                return NotFound();
            }

            int procId = Environment.ProcessId;
            long memoryUsed = Environment.WorkingSet;

            decimal freeSpace = 0;
            decimal driveSize = 0;
            DriveInfo[] allDrives = DriveInfo.GetDrives();
            foreach(var drive in allDrives)
            {
                freeSpace += drive.TotalFreeSpace / 1024 / 1024 / 1024;
                driveSize += drive.TotalSize / 1024 / 1024 / 1024;
            }

            AdminIndexViewModel model = new AdminIndexViewModel();
            model.TotalSize = driveSize;
            model.FreeSpace = freeSpace;

            float totalAllocMemInBytes = Process.GetProcesses().Sum(a => a.PrivateMemorySize64) / 1024 / 1024 / 1024;
            model.MemoryUsed = totalAllocMemInBytes;

            model.TotalTasks = dbContext.Cards.Count();
            model.TotalProjects = dbContext.Projects.Count();

            decimal plusPrice = 4.99m;
            decimal proPrice = 9.99m;
            
            // We dont want to count the root user
            model.UserCount = dbContext.Users.Where(user => user.NormalizedUserName != "ROOT" && user.LastVisited != null && user.LastVisited >= DateTime.Today.AddDays(-10)).ToList().Count;
            model.ProjectCount = dbContext.Projects.ToList().Count;
            model.CardCount = dbContext.Cards.ToList().Count;
            model.Section = "Index";

            DateTime end = MonthUtil.GetPreviousMonth(true);
            model.PlusSalesThisMonth = plusPrice * dbContext.Sales.Where(s => s.PaymentTier == Enums.PaymentTier.Plus && s.SaleDate > end && s.SaleIsFree == false).ToList().Count;
            model.ProSalesThisMonth = proPrice * dbContext.Sales.Where(s => s.PaymentTier == Enums.PaymentTier.Pro && s.SaleDate > end && s.SaleIsFree == false).ToList().Count;

            DateTime start = MonthUtil.GetPreviousMonth(false);

            decimal plusSalesLastMonth = plusPrice * dbContext.Sales.Where(s =>  s.PaymentTier == Enums.PaymentTier.Plus && s.SaleDate > start && s.SaleDate < end && s.SaleIsFree == false).ToList().Count;
            decimal proSalesLastMonth = proPrice * dbContext.Sales.Where(s => s.PaymentTier == Enums.PaymentTier.Pro && s.SaleDate > start && s.SaleDate < end && s.SaleIsFree == false).ToList().Count;

            DateTime jan01 = new DateTime(DateTime.Now.Year, 1, 1);

            decimal plusSalesThisYear= plusPrice * dbContext.Sales.Where(s => s.PaymentTier == Enums.PaymentTier.Plus && s.SaleDate > jan01 && s.SaleDate < DateTime.Now && s.SaleIsFree == false).ToList().Count;
            decimal proSalesThisYear = proPrice * dbContext.Sales.Where(s => s.PaymentTier == Enums.PaymentTier.Pro && s.SaleDate > jan01 && s.SaleDate < DateTime.Now && s.SaleIsFree == false).ToList().Count;

            model.TotalSalesThisMonth = model.PlusSalesThisMonth + model.ProSalesThisMonth;
            model.TotalSalesLastMonth = plusSalesLastMonth + proSalesLastMonth;

            GetSalesContactRequests(model);

            return View(model);
        }

        public IActionResult UserManager()
        {
            if (!User.IsInRole(Constants.AdminRole) && !User.IsInRole(Constants.SupportRole))
            {
                _logger.LogWarning($"{User.Identity.Name} tried to access ACP->UserManager");
                return NotFound();
            }

            AdminIndexViewModel model = new AdminIndexViewModel();
            model.Section = "User";
            GetSalesContactRequests(model);
            return View(model);
        }

        public IActionResult ContactRequests()
        {
            if (!User.IsInRole(Constants.AdminRole) && !User.IsInRole(Constants.SalesRole))
            {
                _logger.LogWarning($"{User.Identity.Name} tried to access ACP->ContactRequests");
                return NotFound();
            }

            AdminIndexViewModel model = new AdminIndexViewModel();
            model.Section = "Contact Requests";
            GetSalesContactRequests(model);
            return View(model);
        }

        public IActionResult SeeContact(int id)
        {
            if (!User.IsInRole(Constants.AdminRole) && !User.IsInRole(Constants.SalesRole))
            {
                _logger.LogWarning($"{User.Identity.Name} tried to access ACP->SeeContact");
                return NotFound();
            }

            AdminIndexViewModel model = new AdminIndexViewModel();
            model.Section = "Contact Requests";
            GetSalesContactRequests(model);

            var contact = dbContext.SalesContacts.Where(contact => contact.Id == id).FirstOrDefault();
            if(contact == null)
            {
                return NotFound();
            }

            model.SalesContact = contact;
            return View(model);
        }

        public async Task<IActionResult> OnContacted(int id, bool contacted)
        {
            if (!User.IsInRole(Constants.AdminRole) && !User.IsInRole(Constants.SalesRole))
            {
                _logger.LogWarning($"{User.Identity.Name} tried to access ACP->OnContacted");
                return NotFound();
            }

            var contact = dbContext.SalesContacts.Where(contact => contact.Id == id).FirstOrDefault();
            if(contact == null)
            {
                return BadRequest();
            }

            if(contacted)
            {
                contact.IsContacted = true;
            }
            else
            {
                dbContext.SalesContacts.Remove(contact);
            }
            await dbContext.SaveChangesAsync();
            return RedirectToAction(nameof(ContactRequests));
        }

        private void GetSalesContactRequests(AdminIndexViewModel model)
        {
            if (User.IsInRole(Constants.AdminRole) || User.IsInRole(Constants.SalesRole))
            {
                model.SalesContacts = dbContext.SalesContacts.Where(s => s.IsContacted == false).OrderBy(s => s.Timestamp).ToList();
                Console.WriteLine($"sales = {model.SalesContacts.Count}");
            }
        }

        public IActionResult BlogPosts()
        {
            if (!User.IsInRole(Constants.AdminRole) && !User.IsInRole(Constants.MarketerRole))
            {
                _logger.LogWarning($"{User.Identity.Name} tried to access ACP->BlogPosts");
                return NotFound();
            }

            AdminIndexViewModel model = new AdminIndexViewModel();
            model.Section = "Blog Posts";

            model.BlogPosts = dbContext.BlogPosts.OrderByDescending(s => s.DatePosted).ToList();

            return View(model);
        }

        public async Task<IActionResult> NewPost(int id = 0)
        {
            if (!User.IsInRole(Constants.AdminRole) && !User.IsInRole(Constants.MarketerRole))
            {
                _logger.LogWarning($"{User.Identity.Name} tried to access ACP->NewPost");
                return NotFound();
            }

            WriteBlogPostViewModel model = new WriteBlogPostViewModel();
            model.Input = new WriteBlogPostViewModel.WriteBlogPost();
            var existingPost = await dbContext.BlogPosts.Where(post => post.Id == id).FirstOrDefaultAsync();
            if(existingPost != null)
            {
                model.Input.Title = existingPost.Title;
                model.Input.Content = existingPost.Content;
                model.Input.Summary = existingPost.Summary;
                model.Input.Slug = existingPost.Slug;
                model.Input.Id = existingPost.Id;
                model.Input.Keywords = existingPost.Keywords;
            }
            else
            {
                model.Input.Id = 0;
            }
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> CreateBlogPost(WriteBlogPostViewModel.WriteBlogPost input)
        {
            Console.WriteLine(JsonUtility.ToJson(input, true));
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            string uploadsFolder = m_PathService.GetWebRootPath("uploaded_images");
            var fileName = string.Empty;
            Console.WriteLine($"blogPost.ImageFile is null: {input.Header == null}");
            if (input.Header != null && input.Header.Length > 0)
            {
                // TODO: We need to run an antivirus scan on --ANYTHING-- that gets uploaded to the server

                fileName = $"{Guid.NewGuid().ToString()}_{input.Header.FileName}";
                _logger.LogInformation($"Uploading {fileName} ({input.Header.Length} bytes)...");
                string filePath = Path.Combine(uploadsFolder, fileName);
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await input.Header.CopyToAsync(fileStream);
                }
            }

            if (input.Id > 0)
            {
                var existingPost = await dbContext.BlogPosts.Where(post => post.Id == input.Id).FirstOrDefaultAsync();
                if(existingPost == null)
                {
                    return BadRequest();
                }

                existingPost.DateModified = DateTime.Now;
                existingPost.DatePosted = existingPost.DatePosted;
                existingPost.AuthorId = existingPost.AuthorId;
                existingPost.Image = fileName;
                existingPost.Title = input.Title;
                existingPost.Slug = input.Slug;
                existingPost.Content = input.Content;
                existingPost.Keywords = input.Keywords;
                existingPost.Summary = input.Summary;
                Console.WriteLine(JsonUtility.ToJson(existingPost, true));

                dbContext.ChangeTracker.DetectChanges();
                Console.WriteLine(dbContext.ChangeTracker.DebugView.LongView);

                await dbContext.SaveChangesAsync();
                return RedirectToAction(nameof(BlogPosts));
            }

            var user = await _userManager.GetUserAsync(User);
            var newBlogPost = new BlogPost();
            newBlogPost.AuthorId = Guid.Parse(user.Id);
            newBlogPost.DatePosted = DateTime.Now;
            newBlogPost.Title = input.Title;
            newBlogPost.Summary = input.Summary;
            newBlogPost.Content = input.Content;
            newBlogPost.Slug = input.Slug;
            newBlogPost.Image = fileName;

            await dbContext.BlogPosts.AddAsync(newBlogPost);
            await dbContext.SaveChangesAsync();

            var subbedUsers = dbContext.BlogSubscriptions.ToList();
            foreach(var subUser in subbedUsers)
            {
                string emailContent = $"{newBlogPost.Content}\n\n<a style=\"text-align: center;\" href=\"https://plan-suite.com/unsubscribe/{subUser.Id}\">Unsubscribe</a>";
                await m_EmailSender.SendEmailAsync(subUser.Email, newBlogPost.Title, emailContent);
            }

            return RedirectToAction(nameof(BlogPosts));
        }

        public async Task<IActionResult> DeletePost(int id)
        {
            if (!User.IsInRole(Constants.AdminRole) && !User.IsInRole(Constants.MarketerRole))
            {
                _logger.LogWarning($"{User.Identity.Name} tried to access ACP->DeletePost");
                return NotFound();
            }

            var post = await dbContext.BlogPosts.Where(post => post.Id == id).FirstOrDefaultAsync();
            if (post != null)
            {
                dbContext.BlogPosts.Remove(post);
                await dbContext.SaveChangesAsync();
            }
            return RedirectToAction(nameof(BlogPosts));
        }
    }
}
