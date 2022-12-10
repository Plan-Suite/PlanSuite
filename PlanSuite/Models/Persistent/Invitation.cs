using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PlanSuite.Models.Persistent
{
    [Table("invitations")]
    public class Invitation
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("invite_code")]
        public string Code { get; set; }

        [Column("invite_accepted")]
        public bool Accepted { get; set; }

        [Column("invite_expiry")]
        public DateTime Expiry { get; set; }

        [Column("invite_email")]
        public string Email { get; set; }

        [Column("invite_project")]
        public int Project { get; set; }

        [Column("invite_organisation")]
        public int Organisation { get; set; }
    }
}