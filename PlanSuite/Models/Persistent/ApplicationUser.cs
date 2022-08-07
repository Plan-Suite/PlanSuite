using Microsoft.AspNetCore.Identity;
using PlanSuite.Enums;

namespace PlanSuite.Models.Persistent
{
    public class ApplicationUser : IdentityUser
    {
        public PaymentTier PaymentTier { get; set; }
        public DateTime? PaymentExpiry { get; set; }
        public bool FinishedFirstTimeLogin { get; set; }
        public string? StripeCustomerId { get; set; }
    }
}