using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PlanSuite.Models.Persistent
{
    [Table("password_reset_requests")]
    public class PasswordResetRequest
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("account_id")]
        public Guid AccountId { get; set; }

        [Column("code")]
        public string Code { get; set; }

        [Column("expiry")]
        public DateTime Expiry { get; set; }
    }
}