using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PlanSuite.Models.Persistent
{
    [Table("blog_subscription")]
    public class BlogSubscription
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("email")]
        public string Email { get; set; }

        [Column("date_subscribed")]
        public DateTime DateSubscribed { get; set; }

        [Column("subscription_code")]
        public string Code { get; set; }
    }
}