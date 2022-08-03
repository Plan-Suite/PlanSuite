using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using PlanSuite.Data;
using PlanSuite.Models.Persistent;
using PlanSuite.Services;
using Stripe;
using Stripe.Checkout;
using System.Globalization;
using System.Text;

namespace PlanSuite.Controllers.Api
{
    [Route("api/[Controller]")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private readonly ILogger<ProjectController> m_Logger;
        private readonly UserManager<ApplicationUser> m_UserManager;
        private readonly SignInManager<ApplicationUser> m_SignInManager;
        private readonly ApplicationDbContext m_Database;
        private readonly IEmailSender m_EmailSender;

        public PaymentController(ILogger<ProjectController> logger, UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, ApplicationDbContext database, IEmailSender emailSender)
        {
            m_Logger = logger;
            m_UserManager = userManager;
            m_SignInManager = signInManager;
            m_Database = database;
            m_EmailSender = emailSender;
        }

        public static string GetPaymentReceiptString(Sale sale, int amount)
        {
            int pounds = amount / 100;
            int pence = amount % 100;

            string message = "Hello!<br><br>" +
                "This is your receipt for your latest PlanSuite payment.<br><br>" +
                "-----------------------------------------------------<br>" +
                $"<strong>PlanSuite</strong><br>" +
                $"{sale.SaleDate.Day} {CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(sale.SaleDate.Month)} {sale.SaleDate.Year}<br>" +
                $"Item: {sale.PaymentTier} - Amount: £{pounds}.{pence}<br>" +
                $"-----------------------------------------------------<br>" +
                $"Total: £{pounds}.{pence}<br>" +
                $"-----------------------------------------------------<br><br>" +
                $"Regards,<br>PlanSuite.";

            return message;
        }

        [HttpGet("Success")]
        public async Task<IActionResult> OnSuccess([FromQuery] string sessionId)
        {
            m_Logger.LogInformation($"OnSuccess for {sessionId}");
            var sessionService = new SessionService();
            Session session = await sessionService.GetAsync(sessionId);
            if (session == null)
            {
                return Content("Session not found");
            }

            var customerService = new CustomerService();
            Customer customer = await customerService.GetAsync(session.CustomerId);
            if (customer == null)
            {
                return Content("Customer not found");
            }

            int saleId = int.Parse(session.ClientReferenceId);

            var sale = m_Database.Sales.Where(s => s.Id == saleId).FirstOrDefault();
            if(sale == null)
            {
                return Content("Sale not found");
            }

            sale.SaleState = Enums.SaleState.Success;
            await m_Database.SaveChangesAsync();

            var user = await m_UserManager.FindByIdAsync(sale.OwnerId.ToString());
            if (user == null)
            {
                return Content("User not found");
            }

            user.PaymentExpiry = sale.SaleDate.AddMonths(1);
            user.PaymentTier = sale.PaymentTier;
            await m_UserManager.UpdateAsync(user);

            string message = GetPaymentReceiptString(sale, (int)session.AmountTotal);
            Console.WriteLine(message.Replace("<br>", "\n"));

            await m_EmailSender.SendEmailAsync(user.Email, "PlanSuite Payment Receipt", message);

            return Redirect($"/Join/UpgradeSuccess?saleId={sale.Id}&amount={session.AmountTotal}");
        }

        
    }
}
