using PlanSuite.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PlanSuite.Models.Persistent
{
    [Table("projects")]
    public class Project
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("owner_id")]
        public Guid OwnerId { get; set; }

        /// <summary>
        /// Don't assume every project will be tied to an organisation
        /// </summary>
        [Column("org_id")]
        public int OrganisationId { get; set; }

        [Column("name")]
        public string Name { get; set; }

        [Column("description")]
        public string Description { get; set; }

        [Column("created_date")]
        public DateTime CreatedDate { get; set; }

        [Column("due_date")]
        public DateTime? DueDate { get; set; }

        [Column("client")]
        public string? Client { get; set; }

        [Column("budget")]
        public decimal Budget { get; set; }

        [Column("budget_type")]
        public ProjectBudgetType BudgetType { get; set; }

        [Column("budget_monetary_unit")]
        public string? BudgetMonetaryUnit { get; set; }

        [Column("project_completed")]
        public bool ProjectCompleted { get; set; }

        /*[Column("default_view")]
        public ProjectIndexView DefaultView { get; set; }*/

        /*[Column("approved")]
        public ApprovalType Approved { get; set; }

        [Column("approval_reason")]
        public string Approved { get; set; }*/
    }
}