using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using PlanSuite.Controllers.Api;
using PlanSuite.Data;
using PlanSuite.Models.Persistent;
using Stripe;

namespace PlanSuite.WebHookEvents
{
    public class InvoicePaidEvent : IEvent
    {
        public async Task<bool> OnEvent(Event stripeEvent, ApplicationDbContext database, IEmailSender emailSender, UserManager<ApplicationUser> userManager, string arg1)
        {
            var invoice = stripeEvent.Data.Object as Invoice;
            if (invoice == null)
            {
                Console.WriteLine($"Cannot find invoice for InvoicePaid event");
                return false;
            }

            DateTime dateTime = DateTime.Now;

            var customerId = invoice.CustomerId;

            var user = database.Users.Where(user => user.StripeCustomerId == invoice.CustomerId).FirstOrDefault();
            if (user == null)
            {
                Console.WriteLine($"User is null during InvoicePaid event for {invoice.CustomerId}");
                return false;
            }

            Console.WriteLine($"Creating sale for invoice {invoice.Id}");
            Sale sale = new Sale
            {
                PaymentTier = user.PaymentTier,
                SaleDate = dateTime,
                SaleState = Enums.SaleState.Success
            };

            Console.WriteLine($"Saving sale to database");
            await database.Sales.AddAsync(sale);
            await database.SaveChangesAsync();

            // Send email to customer and customer service  

            Console.WriteLine($"Sending receipt to {user.Email}");
            string message = PaymentController.GetPaymentReceiptString(sale, (int)invoice.Total);

            user.PaymentExpiry = dateTime.AddMonths(1);
            await userManager.UpdateAsync(user);

            Console.WriteLine(message);
            await emailSender.SendEmailAsync(user.Email, "PlanSuite Payment Receipt", message);
            return true;
        }
    }
}
