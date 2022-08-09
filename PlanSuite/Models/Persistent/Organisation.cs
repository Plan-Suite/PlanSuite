using PlanSuite.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PlanSuite.Models.Persistent
{
    [Table("organisations")]
    public class Organisation
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("org_name")]
        public string Name { get; set; }

        [Column("org_desc")]
        public string Description { get; set; }

        [Column("org_tier")]
        public PaymentTier Tier { get; set; }
    }

    [Table("organisation_memberships")]
    public class OrganisationMembership
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("user_id")]
        public Guid UserId { get; set; }

        [Column("organisation_id")]
        public int OrganisationId { get; set; }

        [Column("organisation_role")]
        public ProjectRole Role { get; set; }
    }
}