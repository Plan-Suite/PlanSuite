using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using PlanSuite.Data;
using PlanSuite.Models.Persistent;
using Stripe;

namespace PlanSuite.WebHookEvents
{
    public interface IEvent
    {
        /// <summary>
        /// Event Handler for events that use a database context and email sender
        /// </summary>
        Task<bool> OnEvent(Event stripeEvent, ApplicationDbContext database, IEmailSender emailSender, UserManager<ApplicationUser> userManager, string arg1);
    }
}
