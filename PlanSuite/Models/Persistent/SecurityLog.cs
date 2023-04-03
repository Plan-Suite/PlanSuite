using PlanSuite.Enums;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PlanSuite.Models.Persistent
{
    [Table("security_logs")]
    public class SecurityLog
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("date")]
        public DateTime Date { get; set; }

        [Column("action")]
        public LogAction Action { get; set; }

        [Column("area")]
        public string Area { get; set; }

        [Column("desc")]
        public string Description { get; set; }

        [Column("user_id")]
        public Guid UserId { get; set; }

        [Column("old_value")]
        public string? OldValue { get; set; }

        [Column("new_value")]
        public string? NewValue { get; set; }
    }
}