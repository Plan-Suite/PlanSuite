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
    }
}