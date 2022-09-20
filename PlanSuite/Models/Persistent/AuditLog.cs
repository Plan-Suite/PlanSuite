using PlanSuite.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PlanSuite.Models.Persistent
{
    [Table("audit_logs")]
    public class AuditLog
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("timestamp")]
        public DateTime Timestamp { get; set; }

        [Column("log_category")]
        public AuditLogCategory LogCategory { get; set; }

        [Column("user_id")]
        public Guid UserID { get; set; }

        [Column("target_id")]
        public string TargetID { get; set; }

        [Column("log_type")]
        public AuditLogType LogType { get; set; }
    }
}