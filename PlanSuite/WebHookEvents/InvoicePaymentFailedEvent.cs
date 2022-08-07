using Microsoft.AspNetCore.Identity.UI.Services;
using PlanSuite.Data;
using Stripe;
using System.Globalization;

namespace PlanSuite.WebHookEvents
{
    public class InvoicePaymentFailedEvent : IEvent
    {
        public async Task<bool> OnEvent(Event stripeEvent, ApplicationDbContext database, IEmailSender emailSender, string arg1)
        {
            var invoice = stripeEvent.Data.Object as Invoice;
            if (invoice == null)
            {
                Console.WriteLine($"Cannot find invoice for InvoicePaymentFailed event");
                return false;
            }

            DateTime dateTime = DateTime.Now;

            var user = database.Users.Where(user => user.StripeCustomerId == invoice.CustomerId).FirstOrDefault();
            if (user == null)
            {
                Console.WriteLine($"User is null during InvoicePaymentFailed event for {invoice.CustomerId}");
                return false;
            }

            var customerService = new CustomerService();
            Customer customer = await customerService.GetAsync(user.StripeCustomerId);
            if (customer == null)
            {
                Console.WriteLine($"Customer is null during InvoicePaymentFailed event");
                return false;
            }

            var options = new Stripe.BillingPortal.SessionCreateOptions
            {
                Customer = customer.Id,
                ReturnUrl = arg1 + "/Identity/Account/Manage"
            };

            var serviceSession = new Stripe.BillingPortal.SessionService();
            var portal = await serviceSession.CreateAsync(options);
            if (portal == null)
            {
                Console.WriteLine($"Portal is null during InvoicePaymentFailed event");
                return false;
            }

            // Send email to customer and customer service  
            string billingLink = $"Please review and update your payment details: <a href=\"{portal.Url}\">Update your payment</a><br><br>";

            string message = $"Hello {user.UserName}!<br><br>" +
                $"There's a billing issue on your account which we discovered on {dateTime.Day} {CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(dateTime.Month)} {dateTime.Year}.<br>" +
                billingLink +
                $"Regards,<br>PlanSuite.";

            if(invoice.Subscription != null)
            {
                user.PaymentExpiry = invoice.Subscription.CurrentPeriodEnd;
            }

            await emailSender.SendEmailAsync(user.Email, "PlanSuite ", message);
            return true;
        }
    }
}
