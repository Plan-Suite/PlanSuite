using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PlanSuite.Models.Persistent
{
    [Table("blog_comment")]
    public class BlogComment
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("user")]
        public Guid UserId { get; set; }

        [Column("comment")]
        public string Comment { get; set; }

        [Column("date_posted")]
        public DateTime DatePosted { get; set; }
    }
}