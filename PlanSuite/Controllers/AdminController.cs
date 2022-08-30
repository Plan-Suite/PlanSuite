using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PlanSuite.Data;
using PlanSuite.Models.Persistent;
using PlanSuite.Models.Temporary;
using PlanSuite.Services;
using PlanSuite.Utility;
using System.Diagnostics;

namespace PlanSuite.Controllers
{
    // We want to limit access to anything administrator related for obvious reasons
    [Authorize(Roles = "SuperUser,Administrator,Support,Developer")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext dbContext;
        private readonly ILogger<HomeController> _logger;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly AdminService m_AdminService;

        public AdminController(ApplicationDbContext context, ILogger<HomeController> logger, UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, AdminService adminService)
        {
            dbContext = context;
            _logger = logger;
            _userManager = userManager;
            _signInManager = signInManager;
            m_AdminService = adminService;
        }

        public IActionResult Index()
        {
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

            var gc = GC.GetGCMemoryInfo();
            model.TotalMemory = gc.TotalAvailableMemoryBytes / 1024 / 1024 / 1024;
            model.SystemInfo = $"{System.Runtime.InteropServices.RuntimeInformation.RuntimeIdentifier}";

            decimal plusPrice = 4.99m;
            decimal proPrice = 9.99m;

            // We dont want to count the root user
            model.UserCount = dbContext.Users.Where(user => user.NormalizedUserName != "ROOT").ToList().Count;
            model.ProjectCount = dbContext.Projects.ToList().Count;
            model.CardCount = dbContext.Cards.ToList().Count;
            model.Section = "Index";

            DateTime end = MonthUtil.GetPreviousMonth(true);
            model.PlusSalesThisMonth = plusPrice * dbContext.Sales.Where(s => s.PaymentTier == Enums.PaymentTier.Plus && s.SaleDate > end).ToList().Count;
            model.ProSalesThisMonth = proPrice * dbContext.Sales.Where(s => s.PaymentTier == Enums.PaymentTier.Pro && s.SaleDate > end).ToList().Count;

            DateTime start = MonthUtil.GetPreviousMonth(false);

            decimal plusSalesLastMonth = plusPrice * dbContext.Sales.Where(s =>  s.PaymentTier == Enums.PaymentTier.Plus && s.SaleDate > start && s.SaleDate < end).ToList().Count;
            decimal proSalesLastMonth = proPrice * dbContext.Sales.Where(s => s.PaymentTier == Enums.PaymentTier.Pro && s.SaleDate > start && s.SaleDate < end).ToList().Count;

            DateTime jan01 = new DateTime(DateTime.Now.Year, 1, 1);

            decimal plusSalesThisYear= plusPrice * dbContext.Sales.Where(s => s.PaymentTier == Enums.PaymentTier.Plus && s.SaleDate > jan01 && s.SaleDate < DateTime.Now).ToList().Count;
            decimal proSalesThisYear = proPrice * dbContext.Sales.Where(s => s.PaymentTier == Enums.PaymentTier.Pro && s.SaleDate > jan01 && s.SaleDate < DateTime.Now).ToList().Count;

            model.TotalSalesThisMonth = model.PlusSalesThisMonth + model.ProSalesThisMonth;
            model.TotalSalesLastMonth = plusSalesLastMonth + proSalesLastMonth;

            return View(model);
        }

        public IActionResult User()
        {
            AdminIndexViewModel model = new AdminIndexViewModel();
            model.Section = "User";

            return View(model);
        }
    }
}
