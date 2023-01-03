using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc.Routing;
using PlanSuite.Data;
using PlanSuite.Enums;
using PlanSuite.Models.Persistent;
using PlanSuite.Models.Temporary;
using PlanSuite.Utility;
using Stripe;
using System.Security.Policy;

namespace PlanSuite.Services
{
    public class AdminService
    {
        private readonly ApplicationDbContext m_Database;
        private readonly UserManager<ApplicationUser> m_UserManager;
        private readonly SignInManager<ApplicationUser> m_SignInManager;
        private readonly RoleManager<IdentityRole> m_RoleManager;
        private readonly IEmailSender m_EmailSender;
        private readonly ILogger<AdminService> m_Logger;

        public AdminService(ApplicationDbContext dbContext, UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, RoleManager<IdentityRole> roleManager, IEmailSender emailSender, ILogger<AdminService> logger)
        {
            m_Database = dbContext;
            m_UserManager = userManager;
            m_SignInManager = signInManager;
            m_RoleManager = roleManager;
            m_EmailSender = emailSender;
            m_Logger = logger;
        }

        public async Task<GetUsersModel> GetUser(string? username, string? email)
        {
            m_Logger.LogInformation($"AdminService.GetUser: {username}, {email}");
            var appUsers = m_Database.Users.Where(user => 
            user.UserName == username 
            || (username != null && user.UserName.StartsWith(username))
            || (email != null && user.Email.StartsWith(email))).ToList();

            GetUsersModel usersModel = new GetUsersModel();
            if (appUsers.Count < 1)
            {
                return usersModel;
            }

            List<GetUserModel> userModelList = new List<GetUserModel>();
            foreach (var user in appUsers)
            {
                var userModel = new GetUserModel()
                {
                    UserId = Guid.Parse(user.Id),
                    Username = user.UserName,
                    Email = user.Email,
                    PaymentTier = user.PaymentTier
                };

                var appUser = await m_UserManager.FindByIdAsync(user.Id);
                var role = await m_UserManager.GetRolesAsync(appUser);
                userModel.Roles = role.ToArray();
                userModelList.Add(userModel);
            }
            usersModel.GetUserModels = userModelList.ToArray();
            return usersModel;
        }

        public async Task<bool> GiveAdmin(GiveAdminModel model)
        {
            m_Logger.LogInformation($"AdminService.GiveAdmin: {model.Id}");
            var user = await m_UserManager.FindByIdAsync(model.Id);
            if (user == null)
            {
                return false;
            }

            // creating Creating Manager role     
            bool roleExists = await m_RoleManager.RoleExistsAsync(Constants.AdminRole);
            if (!roleExists)
            {
                var role = new IdentityRole();
                role.Name = Constants.AdminRole;
                await m_RoleManager.CreateAsync(role);

                // Create superuser account

                var root = new ApplicationUser();
                root.UserName = "root";
                root.Email = "noreply@plan-suite.com";

                string rootPassword = PasswordReset.GenerateRandomString();

                IdentityResult chkUser = await m_UserManager.CreateAsync(root, rootPassword);

                //Add default User to Role Admin    
                if (chkUser.Succeeded)
                {
                    await m_UserManager.AddToRoleAsync(root, Constants.AdminRole);
                    var token = await m_UserManager.GenerateEmailConfirmationTokenAsync(root);
                    await m_UserManager.ConfirmEmailAsync(root, token);
                }
            }

            bool hasRole = await m_UserManager.IsInRoleAsync(user, Constants.AdminRole);
            if (!hasRole)
            {
                await m_UserManager.AddToRoleAsync(user, Constants.AdminRole);
                Console.WriteLine($"SECURITY: Added {user.FullName} to {Constants.AdminRole}");
            }
            else
            {
                await m_UserManager.RemoveFromRoleAsync(user, Constants.AdminRole);
                Console.WriteLine($"SECURITY: Removed {user.FullName} from {Constants.AdminRole}");
            }

            return true;
        }

        public async Task<bool> SetRole(SetRoleModel model)
        {
            m_Logger.LogInformation($"AdminService.SetRole: {model.Id}, {model.Role}");
            var user = await m_UserManager.FindByIdAsync(model.Id);
            if (user == null)
            {
                return false;
            }

            PaymentTier paymentTier = (PaymentTier)model.Role; ;
            DateTime now = DateTime.Now;

            var sale = new Sale();
            sale.OwnerId = Guid.Parse(user.Id);
            sale.PaymentTier = paymentTier;
            sale.SaleDate = now;
            sale.SaleState = Enums.SaleState.Success;
            sale.SaleIsFree = true;
            await m_Database.Sales.AddAsync(sale);
            await m_Database.SaveChangesAsync();

            user.PaymentExpiry = now.AddMonths(1);
            user.PaymentTier = paymentTier;
            await m_UserManager.UpdateAsync(user);

            // Generate the payment receipt
            string message = PlanSuite.Controllers.Api.PaymentController.GetPaymentReceiptString(sale, 0);
            await m_EmailSender.SendEmailAsync(user.Email, "PlanSuite Payment Receipt", message);
            return true;
        }

        public async Task<bool> SendPasswordReset(SendPasswordResetModel model)
        {
            m_Logger.LogInformation($"AdminService.SendPasswordReset: {model.Id}");
            var user = await m_UserManager.FindByIdAsync(model.Id);
            if (user == null)
            {
                return false;
            }

            bool result = await PasswordReset.SendPasswordReset(user, m_UserManager);
            if(!result)
            {
                return false;
            }
            return true;
        }

        public async Task<bool> ModifyUser(SaveUserModel model)
        {
            m_Logger.LogInformation($"AdminService.ModifyUser: {model.Id}, New email: {model.NewEmail}, new name: {model.NewName}");
            var user = await m_UserManager.FindByIdAsync(model.Id);
            if (user != null)
            {
                if(!string.IsNullOrEmpty(model.NewName))
                {
                    m_Logger.LogInformation($"{user.FullName}: Modified username to {model.NewName}");
                    await m_UserManager.SetUserNameAsync(user, model.NewName);
                    await m_UserManager.UpdateNormalizedUserNameAsync(user);
                }
                if (!string.IsNullOrEmpty(model.NewEmail))
                {
                    m_Logger.LogInformation($"{user.FullName}: Modified email to {model.NewName}");
                    await m_UserManager.SetEmailAsync(user, model.NewEmail);
                    var token = await m_UserManager.GenerateEmailConfirmationTokenAsync(user);
                    await m_UserManager.ConfirmEmailAsync(user, token);
                }

                await m_UserManager.UpdateAsync(user);
                m_Database.Entry(user).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                await m_Database.SaveChangesAsync();

                await m_SignInManager.RefreshSignInAsync(user);
                return true;
            }
            return false;
        }
    }
}
