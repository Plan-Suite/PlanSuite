using PlanSuite.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PlanSuite.Models.Persistent
{
    [Table("projects_access")]
    public class ProjectAccess
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("user_id")]
        public Guid UserId { get; set; }

        [Column("project_id")]
        public int ProjectId { get; set; }

        [Column("access_since")]
        public DateTime AccessSince { get; set; }

        [Column("project_role")]
        public ProjectRole ProjectRole { get; set; }
    }
}