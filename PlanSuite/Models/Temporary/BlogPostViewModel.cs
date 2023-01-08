using System.ComponentModel.DataAnnotations;

namespace PlanSuite.Models.Temporary
{
    public class BlogPostViewModel
    {
        public string Title { get; set; }
        public string Content { get; set; }
        public int Id { get; set; }
        public string Author { get; set; }
        public DateTime DatePublished { get; set; }
        public string? ImageSrc { get; set; }
        public string? ImageAlt { get; set; }
        public string Summary { get; set; }
        public string Keywords { get; set; }
        public bool IsSubscribed { get; set; }

        public SubscribeInput Subscribe { get; set; } = new SubscribeInput();

        public class SubscribeInput
        {
            [Required]
            [DataType(DataType.EmailAddress)]
            public string Email { get; set; }
        }
    }
}
