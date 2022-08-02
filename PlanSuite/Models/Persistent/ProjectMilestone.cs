using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PlanSuite.Models.Persistent
{
    [Table("project_milestones")]
    public class ProjectMilestone
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("project_id")]
        public int ProjectId { get; set; }

        [Column("title")]
        public string Title { get; set; }

        [Column("description")]
        public string Description { get; set; }

        [Column("due_date")]
        public DateTime? DueDate { get; set; }

        [Column("last_updated")]
        public DateTime LastUpdated { get; set; }

        [Column("is_closed")]
        public bool IsClosed { get; set; }
    }
}