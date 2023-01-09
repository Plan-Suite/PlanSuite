using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PlanSuite.Models.Persistent
{
    [Table("blog_category")]
    public class BlogCategory
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("title")]
        public string Title { get; set; }

        [Column("slug")]
        public string Slug { get; set; }

        /// <summary>
        /// Actual URL will be: https://plan-suite.com/blog/category/post-slug-goes-here
        /// </summary>
        [NotMapped]
        public string Url
        {
            get
            {
                return $"/category/{Slug}";
            }
        }
    }
}