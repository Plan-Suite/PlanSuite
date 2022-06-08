using Microsoft.AspNetCore.Identity;
using PlanSuite.Enums;

namespace PlanSuite.Models.Persistent
{
    public class ApplicationUser : IdentityUser
    {
        public PaymentTier PaymentTier { get; set; }
        //public Language Language { get; set; }
    }
}