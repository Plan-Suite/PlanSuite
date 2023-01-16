using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace PlanSuite.Models.Temporary
{
    public class AddCardModel
    {
        public int ColumnId { get; set; }
        public int ProjectId { get; set; }
        public string Name { get; set; }
    }

    public class WriteBlogPostViewModel
    {
        public WriteBlogPost Input { get; set; } = new WriteBlogPost();

        public class WriteBlogPost
        {
            public int Id { get; set; }
            
            public IFormFile? Header { get; set; }

            [Required]
            public string Title { get; set; }

            [Required]
            public string Slug { get; set; }

            [Required]
            public string Summary { get; set; }

            [Required]
            public string Content { get; set; }

            [Required]
            public string Keywords { get; set; }
        }
    }
}
