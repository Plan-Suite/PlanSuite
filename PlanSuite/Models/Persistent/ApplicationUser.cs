using Microsoft.AspNetCore.Identity;
using PlanSuite.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace PlanSuite.Models.Persistent
{
    public class ApplicationUser : IdentityUser
    {
        [NotMapped]
        public Guid UserId => Guid.Parse(Id);

        public PaymentTier PaymentTier { get; set; }
        public DateTime? PaymentExpiry { get; set; }
        public bool FinishedFirstTimeLogin { get; set; }
        public string? StripeCustomerId { get; set; }
        public DateTime? RegistrationDate { get; set; }
        public DateTime? LastVisited { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }

        [NotMapped]
        public string FullName => $"{FirstName} {LastName}";
    }
}