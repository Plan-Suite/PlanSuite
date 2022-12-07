using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PlanSuite.Models.Persistent
{
    [Table("sales_contacts")]
    public class SalesContact
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("first_name")]
        public string FirstName { get; set; }

        [Column("last_name")]
        public string LastName { get; set; }

        [Column("email")]
        public string Email { get; set; }

        [Column("job_title")]
        public string JobTitle { get; set; }

        [Column("phone_number")]
        public string PhoneNumber { get; set; }

        [Column("message")]
        public string Message { get; set; }

        [Column("timestamp")]
        public DateTime Timestamp { get; set; }

        [Column("is_contacted")]
        public bool IsContacted { get; set; }
    }
}