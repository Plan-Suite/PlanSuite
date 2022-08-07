using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using PlanSuite.Data;
using PlanSuite.WebHookEvents;
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
        private readonly string m_EndpointSecret;

        public PaymentController(ILogger<ProjectController> logger, UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, ApplicationDbContext database, IEmailSender emailSender)
        {
            m_Logger = logger;
            m_UserManager = userManager;
            m_SignInManager = signInManager;
            m_Database = database;
            m_EmailSender = emailSender;
            m_EndpointSecret = PaymentService.Secret;
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
            user.StripeCustomerId = customer.Id;
            await m_UserManager.UpdateAsync(user);

            string message = GetPaymentReceiptString(sale, (int)session.AmountTotal);
            Console.WriteLine(message.Replace("<br>", "\n"));

            await m_EmailSender.SendEmailAsync(user.Email, "PlanSuite Payment Receipt", message);

            return Redirect($"/Join/UpgradeSuccess?saleId={sale.Id}&amount={session.AmountTotal}");
        }

        [HttpPost("WebHookEvent")]
        public async Task<IActionResult> OnWebHookEvent()
        {
            m_Logger.LogInformation("OnWebHookEvent start");

            var domain = $"https://{HttpContext.Request.Host}";
            var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();

            try
            {
                var stripeEvent = EventUtility.ConstructEvent(json, Request.Headers["Stripe-Signature"], m_EndpointSecret);

                // Handle the event
                if (stripeEvent.Type == Events.InvoicePaymentFailed)
                {
                    m_Logger.LogInformation("Handling InvoicePaymentFailed event");

                    InvoicePaymentFailedEvent invoicePaymentFailed = new InvoicePaymentFailedEvent();
                    await invoicePaymentFailed.OnEvent(stripeEvent, m_Database, m_EmailSender, domain);
                }
                else
                {
                    // Unexpected event type
                    m_Logger.LogWarning($"Unhandled event type: {stripeEvent.Type}");
                }
                return Ok();
            }
            catch (StripeException e)
            {
                m_Logger.LogError($"StripeException: {e.Message}\nStripeError: {e.StripeError.Error}\n\n{e.StripeError.Message}");
                return BadRequest();
            }
        }
    }
}
